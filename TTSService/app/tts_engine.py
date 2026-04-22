import asyncio
import os
from pathlib import Path
import edge_tts
from .models import SUPPORTED_LANGUAGES


async def generate_audio(poi_id: str, language: str, text: str, storage_path: str) -> str:
    """Generate MP3 audio for a POI in the given language. Returns the relative audio URL."""
    voice = SUPPORTED_LANGUAGES[language]
    poi_dir = Path(storage_path) / poi_id
    poi_dir.mkdir(parents=True, exist_ok=True)

    output_file = poi_dir / f"{language}.mp3"
    communicate = edge_tts.Communicate(text, voice)
    await communicate.save(str(output_file))

    return f"/audio/{poi_id}/{language}.mp3"
