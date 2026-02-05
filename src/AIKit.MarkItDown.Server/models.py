from pydantic import BaseModel
from typing import Optional, Dict, Any, List

# Pydantic models
class MarkDownConfig(BaseModel):
    docintel_endpoint: Optional[str] = None
    llm_api_key: Optional[str] = None
    llm_model: Optional[str] = None
    llm_prompt: Optional[str] = None
    keep_data_uris: Optional[bool] = None
    enable_plugins: Optional[bool] = None
    docintel_key: Optional[str] = None

class MarkDownResult(BaseModel):
    text: str
    title: Optional[str] = None
    metadata: Dict[str, Any] = {}

class ConvertUriRequest(BaseModel):
    uri: str
    config: Optional[MarkDownConfig] = None