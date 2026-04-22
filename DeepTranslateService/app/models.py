from pydantic import BaseModel, Field

class TranslateRequest(BaseModel):
    text: str = Field(..., min_length=1, max_length=5000)
    source_language: str = Field(default="vi", min_length=2, max_length=10)
    target_language: str = Field(..., min_length=2, max_length=10)

class TranslateResponse(BaseModel):
    translated_text: str
    source_language: str
    target_language: str
    provider: str