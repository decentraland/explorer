namespace DCL
{
    public static class ContentServerUtils
    {
        public enum ApiEnvironment
        {
            NONE,
            TODAY,
            ZONE,
            ORG,
        }


        public static string GetEnvString(ApiEnvironment env)
        {
            switch (env)
            {
                case ApiEnvironment.NONE:
                    break;
                case ApiEnvironment.TODAY:
                    return "today";
                case ApiEnvironment.ZONE:
                    return "zone";
                case ApiEnvironment.ORG:
                    return "org";
            }

            return "org";
        }

        public static string GetScenesAPIUrl(ApiEnvironment env, int x1, int y1, int width, int height)
        {
            string envString = GetEnvString(env);
            return $"https://content.decentraland.{envString}/scenes?x1={x1}&x2={x1 + width}&y1={y1}&y2={y1 + height}";
        }

        public static string GetMappingsAPIUrl(ApiEnvironment env, string cid)
        {
            string envString = GetEnvString(env);
            return $"https://content.decentraland.{envString}/parcel_info?cids={cid}";
        }

        public static string GetContentAPIUrlBase(ApiEnvironment env)
        {
            string envString = GetEnvString(env);
            return $"https://content.decentraland.{envString}/contents/";
        }

        public static string GetBundlesAPIUrlBase(ApiEnvironment env)
        {
            return "http://localhost:1338/";
            string envString = GetEnvString(env);
            return $"https://content-as-bundle.decentraland.{envString}/contents/";
        }
    }
}
