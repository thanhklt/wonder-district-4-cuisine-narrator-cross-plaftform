import os
import deepl

_translator = deepl.Translator(os.environ["DEEPL_API_KEY"])

LANGUAGE_MAP = {
    "vi": "VI",
    "en": "EN-US",
    "zh": "ZH-HANS",
    "ja": "JA",
    "ru": "RU",
    "ko": "KO",
    "fr": "FR",
    "de": "DE",
    "es": "ES",
    "th": "TH",
}

def translate_text(text: str, source_language: str, target_language: str) -> str:
    if not text.strip():
        return ""

    if source_language.lower() == target_language.lower():
        return text

    src = LANGUAGE_MAP.get(source_language.lower())
    tgt = LANGUAGE_MAP.get(target_language.lower())

    if not tgt:
        raise ValueError(f"Unsupported target language: {target_language}")

    result = _translator.translate_text(text, source_lang=src, target_lang=tgt)
    return str(result) if result else ""