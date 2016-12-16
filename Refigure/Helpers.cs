namespace Refigure
{
    public static class Helpers
    {
        ///// <summary>
        ///// Serializes the objects and then compares them
        ///// </summary>
        ///// <param name="useNewtonSoft">True if you want to use the NewtonSoft Json.NET serializer,
        ///// false if you want to use the built-in .NET serializer</param>
        //public static bool AreEqualByJson(object expected, object actual, bool useNewtonSoft = true)
        //{
        //    if (useNewtonSoft)
        //    {
        //        var expectedJson = JsonConvert.SerializeObject(expected);
        //        var actualJson = JsonConvert.SerializeObject(actual);
        //        return expectedJson == actualJson;
        //    }
        //    else
        //    {
        //        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //        var expectedJson = serializer.Serialize(expected);
        //        var actualJson = serializer.Serialize(actual);
        //        return expectedJson == actualJson;
        //    }
        //}

        //public static string JsonSerialize(object obj)
        //{
        //    return JsonConvert.SerializeObject(obj);
        //}

        //public static object JsonDeserialize(string json)
        //{
        //    return JsonConvert.DeserializeObject(json);
        //}

        ///// <summary>
        ///// Merges the calling path in front of the given path.
        ///// </summary>
        ///// <param name="path1">Left side of path</param>
        ///// <param name="path2">Right side of path</param>
        ///// <returns></returns>
        //public static string MergePath(this string path1, string path2, bool forwardSlash = false)
        //{
        //    if (path1.EndsWith(forwardSlash ? @"/" : @"\"))
        //    {
        //        if (path2.StartsWith(forwardSlash ? @"/" : @"\"))
        //        {
        //            return path1.Remove(path1.Length - 1) + path2;
        //        }
        //        return path1 + path2;
        //    }
        //    else
        //    {
        //        if (path2.StartsWith(forwardSlash ? @"/" : @"\"))
        //        {
        //            return path1 + path2;
        //        }
        //        return path1 + (forwardSlash ? @"/" : @"\") + path2;
        //    }
        //}

        ///// <summary>
        ///// Merges two URL paths.
        ///// </summary>
        //public static string MergeURL(this string path1, string path2)
        //{
        //    return path1.MergePath(path2, true);
        //}
    }
}
