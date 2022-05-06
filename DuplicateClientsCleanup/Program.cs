// See https://aka.ms/new-console-template for more information
using LumenWorks.Framework.IO.Csv;
using System.Data;
using System.Diagnostics;

var csvTable = new DataTable();
char delimiter = ',';

using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"C:\rawnew.csv")), true, delimiter: delimiter ))
{
    csvTable.Load(csvReader);
}

List<ClientModel> clients = new List<ClientModel>();
Dictionary<string, List<ClientModel>> clientDict = new Dictionary<string, List<ClientModel>>();
List<ClientModel> clientIdsToDelete= new List<ClientModel>();
List<ClientModel> clientIdsToReview = new List<ClientModel>();
List<ClientModel> clientIdsToKeep = new List<ClientModel>();

try
{
    for (int i = 0; i < csvTable.Rows.Count; i++)
    {
        var reader = new ClientModel
        {
            ClientId = Convert.ToInt32(csvTable.Rows[i][1]),
            FirstName = csvTable.Rows[i][2].ToString(),
            MiddleName = csvTable.Rows[i][3].ToString(),
            LastCorpName = csvTable.Rows[i][4].ToString(),
            AccountNumber = csvTable.Rows[i][5].ToString(),
            IsLinked = csvTable.Rows[i][6].ToString(),
            LinkedAccountNumber = csvTable.Rows[i][7].ToString(),
            TaskId = string.IsNullOrWhiteSpace(csvTable.Rows[i][8].ToString()) ? 0 : Convert.ToInt32(csvTable.Rows[i][8])                 
        };

        if (!clientDict.ContainsKey(reader.AccountNumber)){
            clientDict.Add(reader.AccountNumber, new List<ClientModel> { reader });
        }
        else
        {
            clientDict[reader.AccountNumber].Add(reader);
        }
    }
    // There is a case where Account numbers are null in CSV, Ignoring them
    clientDict.Remove("");
}
catch (Exception ex)
{
    Console.WriteLine("Error Parsing CSV: "+ ex.Message);
}


    
foreach (var account in clientDict)
{
    Debug.WriteLine($"Account Number : {account.Key}");
    Debug.WriteLine("");
    var shouldDelete = false;
    var totalCount = account.Value.Count; 
    Debug.WriteLine($"Total Clients : {totalCount} ");
    var countOfClientsWithoutTask = account.Value.Where(x => x.TaskId == 0).ToList().Count;
    Debug.WriteLine($"Clients Without Task : {countOfClientsWithoutTask}");
    var countOfClientsWithTask = account.Value.Where(x => x.TaskId != 0).ToList().Count;
    Debug.WriteLine($"Clients With Task : {countOfClientsWithTask}");

    bool areNamesSimilar = checkSimilarNames(account.Value);
    
    if (countOfClientsWithTask == totalCount)
    {
        // every client for this account has a task -- do not delete -- mark for review
        Debug.WriteLine("Do not Delete - Marked for Review");
        foreach(var client in account.Value)
        {
            clientIdsToReview.Add(client);
        }
    }
    else if (countOfClientsWithoutTask == totalCount)
    {
        //every client for this account doesnt have a task -- delete only unlinked client
        Debug.WriteLine("Delete Only unlinked Client");
        foreach (var client in account.Value)
        {
            if(client.IsLinked == "N" && areNamesSimilar )
            {
                clientIdsToDelete.Add(client);
            }
            else
            {
                clientIdsToKeep.Add(client);
            }
        }
    }
    else
    {
        Debug.WriteLine("Delete Clients that dont have a task");
        // some of the clients have a task
        // delete only the clients without task
        foreach (var client in account.Value)
        {
            if(client.TaskId == 0 && areNamesSimilar)
            {
                clientIdsToDelete.Add(client);
            }
            else
            {
                clientIdsToKeep.Add(client);
            }
        }
    }
    Debug.WriteLine($"-------------------------------------------------");
}

bool checkSimilarNames(List<ClientModel> clientModel)
{
    // Remove ',' from the end
    foreach (var client in clientModel)
    {
        var name = client.FirstName+client.MiddleName+client.LastCorpName;
        name = name.TrimEnd();
        if (name.EndsWith(','))
        {
            name = name.Remove(name.Length - 1);
        }
        client.FullName = name.ToLower();

    }
    return clientModel.All(x => x.FullName == clientModel.First().FullName);
}

Console.WriteLine($"Records to Keep {clientIdsToKeep.Count}");
foreach (var client in clientIdsToKeep)
{
    Console.WriteLine($" Client ID : {client.ClientId} | Name: {client.LastCorpName} | Account number : {client.AccountNumber} | Task ID : {client.TaskId}");
}

Console.WriteLine($"Records to Review {clientIdsToReview.Count}");
foreach (var client in clientIdsToReview)
{
    Console.WriteLine($" Client ID : {client.ClientId} | Name: {client.LastCorpName} | Account number : {client.AccountNumber} | Task ID : {client.TaskId}");
}

Console.WriteLine($"Number of Clients to Delete {clientIdsToDelete.Count}");
foreach (var client in clientIdsToDelete)
{
    Console.WriteLine($" Client ID : {client.ClientId} | Name: {client.LastCorpName} | Account number : {client.AccountNumber} | Task ID : {client.TaskId}");
}

Console.WriteLine($"-------------------------------------------------");

Console.WriteLine("Delete Query");
var list = string.Join(",", clientIdsToDelete.Select(x => x.ClientId.ToString()));
Console.WriteLine($"delete from client_master where client_id in ({list})");

Console.WriteLine($"-------------------------------------------------");


public class ClientModel
{
    public int ClientId { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastCorpName { get; set; }
    public string AccountNumber { get; set; }
    public string IsLinked { get; set; }
    public string LinkedAccountNumber { get; set; }
    public int TaskId { get; set; }
    public string FullName { get; set; }
}

public class Account
{
    public ClientModel ClientObj { get; set; } 
    public string AccountNumber { get; set; }
    public bool ShouldDelete { get; set; }
}