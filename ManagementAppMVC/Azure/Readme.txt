How to create a Service Principal for PowerBI API?
---------------------------------------------------
Is it possible to create a web applictaion which display power bi reports in microsoft fabric and allows roles based access?

Yes — and not only is it possible, it’s actually a very common architecture when working with Microsoft Fabric and Power BI.

How to add service principal so that to get access token in C# code?

Here are the steps how to create a service principal:
	1. Open Azure porttal -> App Registrations -> (+) New Registration
	2. Give some name (for example sp-learnlight-fabric-integration) 
	3. Go Manage -> API permissions -> (+) Add a permission
	4. Look for Power BI Service box.
	5. Click on Application permissions box and select Tenant.Read.All
	6. Ask adminsistrator to consent all permissions.

Additionally (This is made by Azzure Administrator):
Go to: Power BI Admin Portal
Enable for this service principal:
	1. Allow service principals to use Power BI APIs
	2. Allow service principals to access read-only APIs