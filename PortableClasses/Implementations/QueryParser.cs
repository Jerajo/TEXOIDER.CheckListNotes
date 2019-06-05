namespace PortableClasses.Implementations
{
    using System.Collections.Generic;
    public static class QueryParser
    {
        public static Dictionary<string, string> Parse(this string args)
        {
            var arguments = new Dictionary<string, string>();
            var queries = args.Split('&');
            foreach (var query in queries)
            {
                var argument = query.Split('=');
                arguments[argument[0]] = argument[1];
            }
            return arguments;
        }
    }
}
