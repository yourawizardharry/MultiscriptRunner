# MultiscriptRunner

Tool to be used for connecting to Azure SQL servers and enabling SQL batch execution against any number of databases on the target server. This tool can perform CRUD operations and will display all results in a DataTable, which can also be exported as a CSV.

Previously I was using Redgate's solution to acheive this, although I didn't want to pay for a license so I created my own free tool.

Please note that you will need to use the SQL servers master login in order for this tool to work, as it acheives this function by switching the catalog thoughtout execution.

![Alt text](/MultiscriptRunner/Resources/Images/Screenshot.png?raw=true "Screenshot")
