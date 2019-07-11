using System.Collections.Generic;

namespace ConsoleApp1
{
    public class EbRequest
    {
        //         "jsonrpc": "2.0",
        //  "method": "supplier.find",
        //  "params": {
        //      "query" : {
        //          "supplierIds": [],
        //          "cursor": null
        //        }
        //},
        //  "id": 1
        
        public string Jsonrpc { get; set; }
        public string Method { get; set; }
        public Parameters Params { get; set; }
        public int Id { get; set; }
    }

    public class Parameters
    {
        public Query Query { get; set; }
    }

    public class Query
    {
        public string[] SupplierIds { get; set; }
        public string Cursor { get; set; }
    }
}
