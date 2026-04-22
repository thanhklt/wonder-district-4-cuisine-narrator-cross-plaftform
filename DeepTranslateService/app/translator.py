from deep_translator import GoogleTranslator

def translate_text(text: str, source_language: str, target_language: str) -> str:
    if not text.strip():
        return ""

    if source_language.lower() == target_language.lower():
        return text

    result = GoogleTranslator(
        source=source_language,
        target=target_language
    ).translate(text)

    return result or ""