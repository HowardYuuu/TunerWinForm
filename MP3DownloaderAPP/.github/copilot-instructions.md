# Copilot Code Review Instructions

請在所有程式碼審查回覆中一律使用 **繁體中文**。

請在審查時優先關注：
- 程式碼可讀性（Readability）
- 潛在的例外（Exception risks），例如 NullReferenceException
- 命名是否清晰、符合 C#/.NET 慣例
- 是否有魔術數字（magic numbers）
- 是否需要額外加上防呆處理
- 潛在的性能或邏輯問題

避免：
- 過度大型的重構建議
- 會造成風險的破壞性修改除非絕對必要
- 沒有上下文的批評性語言

所有建議請以「工程師對工程師」的口吻給出，保持客觀並提供改善方向。
