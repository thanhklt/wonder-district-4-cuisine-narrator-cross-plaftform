from pydantic import BaseModel, field_validator
from typing import Literal

SUPPORTED_LANGUAGES = {
    "vi": "vi-VN-HoaiMyNeural",
    "en": "en-US-JennyNeural",
    "zh": "zh-CN-XiaoxiaoNeural",
    "ja": "ja-JP-NanamiNeural",
    "ru": "ru-RU-SvetlanaNeural",
}

class TTSRequest(BaseModel):
    poi_id: str
    language: str
    text: str

    @field_validator("language")
    @classmethod
    def validate_language(cls, v: str) -> str:
        if v not in SUPPORTED_LANGUAGES:
            raise ValueError(f"Unsupported language '{v}'. Supported: {list(SUPPORTED_LANGUAGES.keys())}")
        return v

    @field_validator("text")
    @classmethod
    def validate_text(cls, v: str) -> str:
        if not v.strip():
            raise ValueError("text must not be empty")
        if len(v) > 5000:
            raise ValueError("text must be 5000 characters or fewer")
        return v.strip()

    @field_validator("poi_id")
    @classmethod
    def validate_poi_id(cls, v: str) -> str:
        if not v.strip():
            raise ValueError("poi_id must not be empty")
        return v.strip()


class TTSResponse(BaseModel):
    poi_id: str
    language: str
    audio_url: str
    voice: str
