namespace game_vault.aws.lambda.core{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.DataModel;
    using Amazon.DynamoDBv2.DocumentModel;

   
    [DynamoDBTable("csandra_games")]
    public class CsandraGame
    {
        private string id;
        [DynamoDBHashKey] //Partition key
        public string ID {get=> (string.IsNullOrWhiteSpace(id)?"040488110614_"+Spieltitel: id); set=> id = value;}
        public string Spieltitel { get; set; }
        public string Spielerzahl { get; set; }
        public int Komplexitaet { get; set; }
        public string Mechanik { get; set; }
        public int Spielhäufigkeit { get; set; }
        public DateTime? ZuletztGespielt { get; set; }
        public string Dauer { get; set; }

        public string PictureUri{get;set;}
    
        static AmazonDynamoDBClient  SetupDynamoDbClient()=>
            new AmazonDynamoDBClient(
                "AccessKey",
                "Secret",
                new AmazonDynamoDBConfig(){
                    RegionEndpoint = Amazon.RegionEndpoint.EUWest1
                });
        
        internal static async Task<List<CsandraGame>> GetGames (string client=""){
           try{
                using(var context = new DynamoDBContext (SetupDynamoDbClient())){
                    var games = await context.ScanAsync<CsandraGame>(
                        new List<ScanCondition>(){
                            new ScanCondition("ID",ScanOperator.BeginsWith, client )
                            }).GetRemainingAsync();
                    Console.WriteLine("After Scan");
                    
                    return games.ToList();
                }
                
           }
           catch(Exception ex){
               Console.WriteLine(ex.Message);
               return new List<CsandraGame>();
           }
        }

        internal static async Task<string> SaveGame(CsandraGame data, string client = "")  {
            try{
               if(string.IsNullOrWhiteSpace(client))
                    throw new UnauthorizedAccessException("You didn't pass a valid client");
                if(data.ID.Contains(client)){
                    using(var context = new DynamoDBContext (SetupDynamoDbClient())){
                        await context.SaveAsync(data);
                        Console.WriteLine("After Save");
                    }
                    return "";
                }
                else
                    throw new UnauthorizedAccessException("Client is not authorized for changes");
           }
           catch(Exception ex){
               Console.WriteLine(ex.Message);
               return ex.Message;
           }
        }
    }
}