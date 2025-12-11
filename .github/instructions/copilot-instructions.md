# Copilot Code Review Instructions

- When performing a code review, respond in Traditional Chinese (繁體中文) only.
- 在所有程式碼審查回覆中，一律使用繁體中文。

- When performing a code review, focus on:
  - 程式碼可讀性（Readability）
  - 潛在的例外（Exception risks），例如 NullReferenceException
  - 命名是否清晰、符合 C#/.NET 慣例
  - 是否存在魔術數字（magic numbers）
  - 是否需要額外加上防呆處理
  - 潛在的性能或邏輯問題

- Avoid in reviews:
  - 過度大型的重構建議
  - 非必要、具風險的破壞性修改
  - 沒有上下文的批評性語言

- 所有建議請以「工程師對工程師」的口吻給出，保持客觀並提供具體改善方向。
