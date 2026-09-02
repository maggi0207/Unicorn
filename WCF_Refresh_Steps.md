# WCF Service Client Refresh Instructions

Follow these manual steps to refresh and generate the latest WCF service client references from the backend:

1. **Prepare your branch**: 
   Ensure you have a new branch checked out with the latest changes from `main`.

2. **Open Contracts**: 
   Open VS Code and open the relevant service file from the `contracts` folder.

3. **Fetch Latest WSDL**:
   - Navigate to the development service URL in your browser (e.g., `https://dwduiservices-dev.enterprise.wistate.us/SUITESServiceInterfacePortal/...`).
   - Click on the **second WSDL file** link on the page.
   - Right-click the page and select **View Page Source**.

4. **Update XML**: 
   Copy the entire XML source from the browser and use it to replace the contents of the service contract file in VS Code. Save the file.

5. **Generate Client**:
   - Open PowerShell.
   - Navigate to the service client folder (`generated\UI.EmployerPortal.Generated.ServiceClients`).
   - Run the generation script by typing `.\ge` and pressing `Tab` to autocomplete (this will run `.\generate-clients.ps1`).

We have completed the front-end changes for UIEP-2638, implementing the updated modal text and rendering the new Back and Payment History buttons when a bank account cannot be deleted. Currently, the UI detects this condition by checking if the backend error message contains "pending payment", which allows the new UI to work with both the current error text and the upcoming copy from UIEP-3184.

However, relying on string matching in UI logic is fragile and could break if backend wording or formatting changes in the future. We propose adding an explicit ErrorCode (such as "PENDING_PAYMENT_EXISTS") to BankAccountInactivateResponse as part of UIEP-3184. This will cleanly decouple UI control flow from display copy, ensuring future text updates won't affect front-end functionality.