from fastapi import FastAPI, File, UploadFile, HTTPException
from google.cloud import vision
import os, tempfile

# point SDK at the key. keep the JSON next to app.py (or use env var)
os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = "service-account.json"

client = vision.ImageAnnotatorClient()
app = FastAPI()

@app.post("/analyse")
async def analyse(image: UploadFile = File(...)):
    if image.content_type not in ("image/jpeg", "image/png"):
        raise HTTPException(415, "JPEG or PNG only")

    # FastAPI gives a SpooledTemporaryFile; read bytes
    data = await image.read()
    response = client.label_detection(
        image=vision.Image(content=data), max_results=10)

    labels = [a.description for a in response.label_annotations]
    if not labels:
        sentence = "No objects detected."
    elif len(labels) == 1:
        sentence = f"Image may contain {labels[0].lower()}."
    else:
        sentence = ("Image likely contains " +
                    ", ".join(l.lower() for l in labels[:-1]) +
                    f" and {labels[-1].lower()}.")

    return {"description": sentence, "labels": labels}