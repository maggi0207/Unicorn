# Unicorn Project - Agent Skill & Context File

**Purpose:** This file contains critical context, repository limitations, and architectural quirks discovered about the Unicorn project. Provide this file to the AI assistant in future chats to ensure it understands the environment immediately.

## 1. Environment & Repository Limitations
*   **Incomplete Clone:** The AI's local repository is an incomplete copy of the original application.
*   **Do Not Build:** The AI must NEVER attempt to run `dotnet build`, `dotnet run`, or any compilation commands. The local copy will fail to build.
*   **Ask for Missing Files:** If a file is referenced but missing from the local repository, the AI must not guess its contents. It must explicitly ask the user to provide the file or a screenshot from their original application.
*   **Focus on File Issues:** Analyze code statically. Rely entirely on the user to compile, run, and test the application.

## 2. Architecture: WCF Services & Proxies
*   **Frontend Stack:** Blazor Web App (`UI.EmployerPortal.Web`).
*   **Backend Communication:** WCF Services.
*   **Local WSDL Generation:** The WCF proxies (like `Reference.cs`) are NOT generated from a live backend URL. They are generated from **local static XML files** stored in `generated\UI.EmployerPortal.Generated.ServiceClients\Contracts\`.
*   **Updating Proxies:** To pull backend changes, the user MUST:
    1. Get the updated XML (WSDL) file from the backend engineer.
    2. Replace the corresponding file in the local `Contracts` folder.
    3. Run `.\generate-clients.ps1` (ensure the script includes the specific service, e.g., `dotnet svcutil --update EmployerRegistrationService`).

## 3. WCF Serialization Quirks (CRITICAL)
*   **The "Specified" Bug:** If WCF generates a standard value type (like `int`) alongside a boolean `[PropertyName]Specified` property, the frontend MUST set the `Specified` boolean to `true` when mapping the data. Otherwise, WCF will silently strip the property from the network request.
*   **Nullable Types:** If the backend exposes a nullable type (like `int?`) and WCF successfully generates a `System.Nullable<int>` property without a `Specified` boolean, assigning the value directly is sufficient.

## 4. Debugging Workflow & Boundaries
*   **Data Flow Tracing:** When dealing with save/load bugs, meticulously trace the data flow: `Backend Response` -> `Proxy Object` -> `UI Model` -> `Proxy Request` -> `Backend Save`.
*   **Immediate Window:** Use Visual Studio's Immediate Window or breakpoints to inspect whether IDs and data are actually populated in memory *before* WCF serialization.
*   **Backend vs. Frontend Boundaries:** If the frontend is correctly mapping the data to the generated WCF request object (verified via debugger), the frontend's job is complete. Any subsequent failures (e.g., creating duplicates instead of updating) are the responsibility of the backend developer's SQL/Save logic.
