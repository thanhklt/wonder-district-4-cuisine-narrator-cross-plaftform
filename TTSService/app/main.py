import os
from pathlib import Path
from fastapi import FastAPI, HTTPException
from fastapi.staticfiles import StaticFiles
from fastapi.responses import JSONResponse, Response
from dotenv import load_dotenv
from .models import TTSRequest, TTSResponse, TTSStreamRequest
from .tts_engine import generate_audio, stream_audio_bytes

load_dotenv()

AUDIO_STORAGE_PATH = os.getenv("AUDIO_STORAGE_PATH", "/storage/audio")
Path(AUDIO_STORAGE_PATH).mkdir(parents=True, exist_ok=True)

app = FastAPI(title="AudioTravelling TTS Service", version="1.0.0")
app.mount("/audio", StaticFiles(directory=AUDIO_STORAGE_PATH), name="audio")


@app.get("/health")
async def health():
    return {"status": "ok", "service": "tts"}


@app.post("/tts/generate", response_model=TTSResponse)
async def generate(req: TTSRequest):
    try:
        audio_url = await generate_audio(req.poi_id, req.language, req.text, AUDIO_STORAGE_PATH)
        from .models import SUPPORTED_LANGUAGES
        return TTSResponse(
            poi_id=req.poi_id,
            language=req.language,
            audio_url=audio_url,
            voice=SUPPORTED_LANGUAGES[req.language],
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/tts/stream")
async def stream(req: TTSStreamRequest):
    try:
        audio_bytes = await stream_audio_bytes(req.language, req.text)
        if not audio_bytes:
            raise HTTPException(status_code=500, detail="No audio generated")
        return Response(content=audio_bytes, media_type="audio/mpeg")
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
