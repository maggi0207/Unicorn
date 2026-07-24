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
