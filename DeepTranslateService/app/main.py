from fastapi import FastAPI, HTTPException
import uvicorn
from app.models import TranslateRequest, TranslateResponse
from app.translator import translate_text

app = FastAPI(title="DeepTranslate Service")

@app.get("/health")
def health():
    return {"status": "ok"}

@app.post("/translate", response_model=TranslateResponse)
def translate(req: TranslateRequest):
    try:
        translated = translate_text(
            text=req.text,
            source_language=req.source_language,
            target_language=req.target_language
        )

        return TranslateResponse(
            translated_text=translated,
            source_language=req.source_language,
            target_language=req.target_language,
            provider="deep-translator"
        )
    except Exception as ex:
        raise HTTPException(status_code=500, detail=str(ex))
    
if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)