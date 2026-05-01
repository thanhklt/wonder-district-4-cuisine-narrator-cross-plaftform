from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse, Response
from pydantic import BaseModel
import edge_tts
import tempfile
import os

app = FastAPI()

class TtsRequest(BaseModel):
    text: str
    voice: str
    lang: str = "vi"

@app.get("/health")
def health():
    return {"status": "ok"}

@app.post("/tts")
async def generate_tts(req: TtsRequest):
    if not req.text or not req.voice:
        raise HTTPException(status_code=400, detail="Missing text or voice")

    try:
        # Create a temporary file to store the generated mp3
        fd, temp_path = tempfile.mkstemp(suffix=".mp3")
        os.close(fd)

        communicate = edge_tts.Communicate(req.text, req.voice)
        await communicate.save(temp_path)

        with open(temp_path, "rb") as f:
            data = f.read()
            
        # Clean up the temp file
        try:
            os.remove(temp_path)
        except:
            pass

        return Response(content=data, media_type="audio/mpeg")
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
