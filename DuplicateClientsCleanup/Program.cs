// See https://aka.ms/new-console-template for more information


using System.Diagnostics;
using static DuplicateClientsCleanup.Helper;

List<ClientModel> clients = new List<ClientModel>();
Dictionary<string, List<ClientModel>> clientDict = new Dictionary<string, List<ClientModel>>();
List<ClientModel> clientIdsToDelete = new List<ClientModel>();
List<ClientModel> clientIdsToReview = new List<ClientModel>();
List<ClientModel> clientIdsToKeep = new List<ClientModel>();

ParseCsv(clientDict);

foreach (var account in clientDict)
{
    Debug.WriteLine($"Account Number : {account.Key}");
    Debug.WriteLine("");
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
        foreach (var client in account.Value)
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
            if (client.IsLinked == "N" && areNamesSimilar)
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
            if (client.TaskId == 0 && areNamesSimilar)
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
