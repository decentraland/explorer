using DCL.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static DCL.ContentServerUtils;

[assembly: InternalsVisibleTo("AssetBundleBuilderTests")]
namespace DCL
{
    public static class AssetBundleMenuItems
    {
        [System.Serializable]
        public class EmptyParcels
        {
            public MappingPair[] EP_01;
            public MappingPair[] EP_02;
            public MappingPair[] EP_03;
            public MappingPair[] EP_04;
            public MappingPair[] EP_05;
            public MappingPair[] EP_06;
            public MappingPair[] EP_07;
            public MappingPair[] EP_08;
            public MappingPair[] EP_09;
            public MappingPair[] EP_10;
            public MappingPair[] EP_11;
            public MappingPair[] EP_12;
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Empty Parcels")]
        public static void DumpEmptyParcels()
        {
            string indexJsonPath = Application.dataPath;

            indexJsonPath += "/../../kernel/static/loader/empty-scenes/index.json";

            if (!File.Exists(indexJsonPath))
            {
                Debug.LogError("Index.json path doesn't exists! Make sure to 'make watch' first so it gets generated.");
                return;
            }

            string emptyScenes = File.ReadAllText(indexJsonPath);
            var es = JsonUtility.FromJson<EmptyParcels>(emptyScenes);

            List<MappingPair> mappings = new List<MappingPair>();

            mappings.AddRange(es.EP_01);
            mappings.AddRange(es.EP_02);
            mappings.AddRange(es.EP_03);
            mappings.AddRange(es.EP_04);
            mappings.AddRange(es.EP_05);
            mappings.AddRange(es.EP_06);
            mappings.AddRange(es.EP_07);
            mappings.AddRange(es.EP_08);
            mappings.AddRange(es.EP_09);
            mappings.AddRange(es.EP_10);
            mappings.AddRange(es.EP_11);
            mappings.AddRange(es.EP_12);

            var builder = new AssetBundleBuilder();

            string emptyScenesResourcesPath = Application.dataPath;
            emptyScenesResourcesPath += "/../../kernel/static/loader/empty-scenes";

            ContentServerUtils.customBaseUrl = "file://" + emptyScenesResourcesPath;

            builder.environment = ContentServerUtils.ApiEnvironment.NONE;
            builder.skipAlreadyBuiltBundles = true;
            builder.deleteDownloadPathAfterFinished = false;

            UnityGLTF.GLTFImporter.OnGLTFWillLoad += GLTFImporter_OnGLTFWillLoad;

            builder.DownloadAndConvertAssets(mappings.ToArray(), (err) => { UnityGLTF.GLTFImporter.OnGLTFWillLoad -= GLTFImporter_OnGLTFWillLoad; });
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump All Wearables Textures (Non bodies)")]
        public static void DumpWearableTextures()
        {
            string[] textures = {
                "Qma1q4mpb3moQYFSqTLShzWMJ6gMhMSR74cCyg8zgZGCnC",
            "QmaLTjTqYnRSKDETfcejP4EWeHi729wKgYzgwC9hAERm4G",
            "QmaNXeRomdK3ig6hB5rd4rJ9C3kok4b9FC5UaKs63B3ekk",
            "QmaPGCFTsiAPGYkhmDbTcxvM5SyqWyhHraPJ65Rzfp4dWi",
            "QmaZBKp3h9dZgBCqbfdMDYjuPFqVdFXMLTmS5cD98wykfY",
            "Qmbar9bThqMwjzN6LD7KhtSJshMp6ACAVXC3cgLjnoBBin",
            "QmbiVLgN11AaMuGW8Lu3747gwqxpU8wwuuGdWc32vUBeRP",
            "QmbjWc6m9JUMRNBtXKJKKoDmw4cDEgZ96crp7SLkU7mDhf",
            "QmbmS6hy2CyiB3XxDATfJ96pF9ge1KqNYEgeU8kSyUQiRj",
            "QmbQt5kjT5u8Y6TzGXiHwyqMn1FxF7UMEXoxt8opTsciby",
            "QmbWE3gyKGdm6kQUxxSzF1rBky9UBAx2Kt686QcuVdTgWM",
            "QmbYXQxmRVgZLkJLy16XgM11iTV6h5YWwK1sijemX46qCr",
            "Qmbz3QZjWmRS8L6JeGugre2Zm6Fb4x78pVYrvnwS2xziEF",
            "QmbZt4ucT5r2tMzFmJAw3dEBoRm7LarzAr3VeNiFFsTfhU",
            "Qmc7Bj6CyA9W5NyY5BNksgX5jotN3Ahdx8XJqUkFxX23Fk",
            "Qmc7LQtTLzecFH2g2eJRcgZnBUjE9VYWS6agJWvGv4amvT",
            "Qmc7zn1e4guVScmTzZS6PmYSaJo4Y4dzL6yAdTJdjw1hF9",
            "QmcafLqvSpWu9nt1tWk3jUazxAdz4schgWr3HGvtKM8HMY",
            "QmcCtYH1pfsHysvuWdjndqd4Mzu3bgNbRtPECEjWzytgkw",
            "QmcjwBHKkdhQryfNbKuspLN1qbLcMxtik5D91rN1LyQkQS",
            "Qmckm8DfKGjVRWV8SbLmdE6La6AZ3ybEFYZBX7jtpp4Gim",
            "QmcKVJnt6AEZMTnnxapDHQNoRY4P2X4R3SBbdCy44g18C2",
            "QmcyRJHVdYbRPau76q9ch6R7dZ7h8zQvXREwBHAUJaE4Cu",
            "QmdB6Yy7uozAptGtRrNcnCVCyAmPQo1Um3TAHAp1tv74rf",
            "QmdFDnVsaCySMW8pumn9qFBairTwxLFqbmuZJVEZx8VWgL",
            "QmdG4aqspAAksNFyr68sz8XqFcN2ynENaQovJ2BBsyWtD2",
            "QmdhHxosuViop4i85K2wYDcv1Pyu5uYEw5tndX1HagtxVs",
            "QmdHVjonR2TNxQ5azeMYem3F3v9yViqkTUYyVHM9EsmfWj",
            "QmdjzGXaeHdyE5x78Me18ti38G2S8M9becPe6qSVArP9e8",
            "QmdPqHWhGTu6H3Dtn1SgvatmpxWeCR9Dm2Qb2hofAHqg1A",
            "QmdX2pphg7EcmrkBeWR92eXeC8v8GcY8T67ptDbH5ndxBD",
            "QmdyPfi4sRYU3eMFWxeXArnCeQ78sZw7oSGxFrntAPqHhy",
            "QmenmqyYDRAyg1P5jZmr8wfFmtsodjfDvzWsVrGM7mBw7f",
            "QmepaHZWSGzBDAWCSKAdBxtkQKzEHUADvy1uwvT4kHDndh",
            "QmepuvwanooeXqaNcLxGuWe92yc4Z1STbw3MRwUm8C45tw",
            "QmeSiuufQxtZwezma5RMeNp8J1nv4ZFrbWzoWRndpiis7A",
            "QmeTxEdW7kp52RV8vZHsrzqed268D5m41T5dVp9JjLXiEr",
            "QmeU4JV36BPcPDYKv8rR59VsWKuWrSXm29zXk2FTNhQ1NR",
            "Qmf16cQidTNPwViHR2A9YmSbSYXFz7dP7YYxdsNwwHF3fG",
            "Qmf1cgho3xVR6UpygFkgnjPDEKabV4qmKKBkinK2QULvqV",
            "Qmf28utefDBX51n6op1camuz9nW6xFHkkZRSNkvRGJThsx",
            "QmfLhAawqLvLUaTkXXuznRYwaWLUS1XVMiYfvxJtYzfFb3",
            "QmfN6FFVN4tXQHUQ3SaRzLGmeKGqCnirohmztar2n7khxa",
            "QmfNvE3nKmahA5emnBnXN2LzydpYncHVz4xy4piw84Er1D",
            "QmfSW5KpKFhZAbNWzq2UnMJBmbz8p8M7vyJpYVKZ2gmxWq",
            "QmfTs9uEJZ4uEX5JHJCne6EadN3YMZB9tdb77YmByQUGGX",
            "QmfXTXNjChYcCEEqo1o7T91rVbTf8hT4EF4Gq49rjPkzYC",
            "QmfYm4Jpjcv5MyBZMAa9LozxDtoegSLYrCAT3HBfFLiMC6",
            "QmNM7wkfpUDULNJ7FC8AGua7b9hNnUtw9zMR676bt57v6J",
            "QmNN6mvZpeey244qAc1NPqZp3CyvaVvKSa5JMveksi8B5N",
            "QmNNf5JTtxrtpdtRDDBv2G44qu1zmjPbzCRsZnvb5Wy7RT",
            "QmNVtC6HM36qNCHFYK1vC1BoMmLhNTHSNMpszAm8v81QsY",
            "QmNzHc2BMNEih73ceP3doxFX2eWs84vCCDExNkDEL5tZUK",
            "QmP11yu5UQye34nFSLjmifX8Ht3zJXfNZ8uv1Ew6KSYHUL",
            "QmP63tZ9pAP88DPm7YuKg3TKu4rJcdNfuJvo548NkXeDxN",
            "QmP6mh1tLPFAZUPbWoXcZUaRArqhtYsE5QNiwD9CQLx1E7",
            "QmPBeon54iy4sZogzc9PqgoUev7TTWfqXs4wiD9FZbDtC7",
            "QmPeDgQ6TgKiK8er9H8udHUhggBf1gKtrBrYh5m6StjufB",
            "QmPMh5FgJQGRqyoYrgLeBxX7tF4jKsZ1mZct7bKynzyuoo",
            "QmPmtaiV74A6jgwo1KcdWwoyHznizT1EX3RzxV7UyDAxsy",
            "QmPMyY37V8yiqzffJyLfidcAruQZxffcrPVrC9j5dSEm79",
            "QmPr2M5bNJPCr2QNE9FDRRR81JJAiMLEayJju6XLSFKs1B",
            "QmPrNtbwmHyVkheET72vKE6i6CUNbk2K7JR2U4j1DzrbXo",
            "QmPt59PBdQkYZw9A7KRHx8hBbUSXkWfXmxEATvnDavJ7gR",
            "QmPxDe26F4BfnPRBFGhXUiejH2yutni466fpGbNhPhRWqD",
            "QmQCgUnQaKHTz5WdMkY8nME3jhYBP8LWNQtmHqRkmA7Avh",
            "QmQdvgkQnqUcJkKhGp3XnjNZ7Hs6ps1Zj9a1KV7ftVHGZD",
            "QmQErsfExFULBqSAnp4csu11ZzBDrubLTyazpnEUSGxnKi",
            "QmQizpS8TMHbkuo7HD9buq5ki7S41QbXwSidzVz1zamdJ9",
            "QmQj2Nt8s2YNizcb6kmdFCZR7Dao9sDW7te3jXY9xyjSDa",
            "QmQj83QfyQ5YvCrqQCF5u3oPoQRqjmHMa2pPqZBmesbnvq",
            "QmQLA3rK18r2yxKjtGT5stCUN8gCLwsEQPawszxzC24hPV",
            "QmQMF9zhYs4ZgTK2PJkyZaa5XG3CjWiuQLLonuFPAStBU1",
            "QmQqiixxk2AAtzTsPvXfyxU4tpbbsDGhfFwfku3CdmJruH",
            "QmQRF9rj8HrC96pXRpHvC4n2w6FssxStFaqdLu3A23nFVp",
            "QmQrXXoYnnpx3SBA9U78gHvczKXS77cQJDxWtkoK4xKazd",
            "QmQZ6MAyHpGaPpntHJnU3x4o2dUvz5KhvNtWbbzvbAE34T",
            "QmR13dEwDYhAudeoYiYLhYmTjuUphikH494fwC3dnRn6r7",
            "QmRaHnacT5G7oLYTYGsRWZtLXzXuTNEq7gWAcvXRSxfwEU",
            "QmRAymNrDWZ6udwMiEayNZgR623kLHiaKjYCV6wUtBeb3T",
            "QmRm3yzEmQACsKmiFm1JXmmpb62cgCFCoNKtprHty6kcnF",
            "QmRQSdpHpmKEYf4JzEWZCRHZDYBc7RN5cqxvxMcL6sMzTq",
            "QmRrKyedET3zXWQVnEb4NYasBQAbucHrbU16Mtj5uD2Lge",
            "QmRu7fnqtte1gjptkugkLXZxoCfuq419ctsSxQHQRaCzus",
            "QmRxKVwraeAq8NwgJ1TM9VnrG4AaQ7HbATxg7BGAVcUhvH",
            "QmRy1tM8RgaaAGzSdBEt5UtDrfese9Zm3LreBZieSJZtxL",
            "QmRz8wJmyZnmLZSVc1YBv3xwrkiNHdRQLrGZi1kRrLobQ6",
            "QmRZ91fELucbiVpmKzfckWE6xz6UV2bD8EqfpzxuGzxx32",
            "QmRzQbA58Wvk4HHkrPuLiLu8rnggCaJbbFeaEqdLvo4CqX",
            "QmS8ow5tnopyj4KuXGGj8FXh5DLX431fsB3pDPLMNCnuYj",
            "QmS9AXGUcf6V9RggB78mYa7zKF1jrh18n3N48JmZ6SYtHp",
            "QmTAQdTdSLcuYfprcs9W7aL7KLkjtZCcjxQfqqo56S5EXy",
            "QmTD7HxrpdF7cGkVATQnDRDorncaBHvaE8F2Nobi8bwAaf",
            "QmTe5qLjnduSMGWxSZtkzHqpUGnv1eNhnpG17moz4xBDBp",
            "QmTHXXLgeJJiZ4iYB8sv4dTHqXUvivFFMGz8aiQsfTgLRo",
            "QmTjfX82edGpbhQKirqSXS3MpGKLwbe9gL8v2KwYaae3t1",
            "QmTPEH75ni5ZYrXtREE3Ci661cc4cwvTr9hspDN1HEWhov",
            "QmTScrK9Mv7SHZoSxRELuhxtDvDQKbPxbyaz4C7T2yDkrC",
            "QmTsVfs4MZvj54eFQJhQGBcjGZ92fd3TwfsgTjjNcvKzP4",
            "QmTTvvM8SbbKpgiP2ZKmLLuz9ji4jzNkQFFnUekzNGmipC",
            "QmU3omTrtqYBtdHcC2FZjYvyLmVVmBKaaKa52CmBF2bn9U",
            "QmUaQBBn87eViWFxd5A9qepJwz5VczUgQAkiaenRijPe1c",
            "QmUoGEoHyKDujWvBYAryMYHn3Rf3k74Tej5dmiNe9RoBrr",
            "QmUpE32bQ1iCiLSXU69gHvoFkAEo25qh9e16xyN3jYF8Vg",
            "QmUuiz4pY1SvxkzwbzCoHD9xXbuT51E7ZFrD5aC31CjUW2",
            "QmUZ3aeo1X66TZzsf9Brn7eudTzJKw8BQ2KTAqAgkuoizD",
            "QmUZCeot1s4c9HzVJkwwjZT9s9txARSZN3NE48nUMxTQz5",
            "QmV8rtHCvztYEPHA78BybsGsbqder8nTm2uezdnHfNz6Tv",
            "QmVbxn91ZRByw9u5jRSuxzs6wrdnLhMqiTX7LXH27CEngb",
            "QmVGmsPD6KkjuFiz3C1UES7MQVWdVUeVDzGuUupZLVGgHJ",
            "QmVhDRzq95uEbXAuZuJhb5ysCQNPVpYnvDrPKPP3p4KmGW",
            "QmVkJS2PgS1EZXJS76JYY2jCYbj5mCrJCWieZCg3B9RK2B",
            "QmVkyYM2RCgU1mvBTsbfQ6qG75Fa4Zd2MxwaL81FV42fD1",
            "QmVL6MbWzhzxJayLxgN3ekK1q9zfeHh3r9cqeewPfrFGbn",
            "QmVPmsZLaLxsJQZq37mT89xkQjeMS9ejBoMZ7u8paVe6B5",
            "QmVq2nQJSDBVotXazfu1TkCiZs8RPnTG2hxvgusjCKEnFd",
            "QmW4f5g2geLPRUSp3hBh6CRhG3NSmXLCY62xiUKqvL8cX6",
            "QmW86GNt8Khf2Zpq2J584eEz4WqQjpoXCQ5cWyNAFxAW3k",
            "QmWgUHpKKSz8NoYvg4PGLDNgNxzVTeWFQx7jeULPrVHXPa",
            "QmWKgS5Z3DDGRfveLRGniDweraZDf9cb1x7X6ebQXiAm8X",
            "QmWkuotFF1CKUtecVVM4E1bMgnMwYdiCLk6ucCQJknpmYb",
            "QmWLrKJFzDCMGXVCef78SDkMHWB94eHP1ZeXfyci3kphTb",
            "QmWMR45paAvZtYvCTVvt296NBXsHDvcc54pURbqu2qMKje",
            "QmWQ6YCoDPJ3ML2WMvK4NMWafsvpAsvyxf7Yv2mSxWkY1A",
            "QmWvSyafpKiT32YBkhe4kDz3SvJvuUKxJJGx43AAGoh1KT",
            "QmWWtyJcCEfkuikY6Mgi7ysE6CfqmekZbMZUgaCKLngBQw",
            "QmWWZNXLkzqiGajGt3X3cp1ziKdVxUodfjsr1BxnyQRPeZ",
            "QmX5rk9V7LzoZpPiWvXWExmdiTFqmuEnuoqFcKwTLb8JPS",
            "QmX5YgdxNmYyDx9jCGAMYd3cGgMjTqFQQXBjSTWMRutFkE",
            "QmXakgrGQB25xRA6q7UWwjpuU69XQAV11o4jmjuUu8u4GZ",
            "QmXBeMAkB4sUFCMy42tGQ9DQZ1HC8VLZ3r4e7p2GdMRudT",
            "QmXc3aGnxsah33rbHk8eYHvf5A5f4CwE6g3CsxkneQZtbC",
            "QmXM3r4YpGAE7Ndr8vXGLrNs2ESD2FqqwHzcpPUk6AeT2H",
            "QmXMPxnEXJjZ794zHio2Ue6wJVtVkP1ep6Q387hYG2ZYka",
            "QmXoDJzv8YtJGWNthecQiyiCuGEFKSVuD6WGVDyXx8uLUk",
            "QmXQijrrpU5DWSjnLZND71UTankGvkow97DH6DTzK7wheN",
            "QmXqPxhpVaNx7KfiWDjBoufQK28373nbZhNqWbKSwcGwUr",
            "QmXTvQ7rj1pZWsbad67xD9iC9XA8ZhvkvhddyET9bHkVXY",
            "QmXTyL1WsJCY6mx41dY14EYHYphaCdnpgdvepEtbsrfSRf",
            "QmXyrpkyVuwx6eQoyuvBjQ1euRN9kVemMLj9GnzvQe6qG8",
            "QmXZhazdrHwbm4NQP5hPXW661zhQP4riNb4CwaGhM6vwV4",
            "QmY2qV7Quad2fytxFZdCaQFeM8d4R6rCsGJ3HV2hmeqWzv",
            "QmYE6US2MZKmMHUyt9pG1ZzZJFoVVa5fkYrEDySqzFuGHY",
            "QmYExf9trqUe7jncQxchV7V3YS7PiVvzqW4KmLA6y3cb7q",
            "QmYjB8VKjD6ZibVpErKCPgb3T3oHDXWuwj3FTcXsnawHYJ",
            "QmYLXh1vKyXjU9wQK3Rp3dWdjfLJs4J3qYBGfjrRteZtwB",
            "QmYqDxZMYcbZeWWZ8gNLM9y5kZCVEECQ8hJHvdr6eoERUJ",
            "QmYs9ec68TjZkRpjz2Sov2GcNhP5AD8JUNGm3MrahpF9RB",
            "QmYsSia1G9PyFgkR6N72JmxrNYQxqa5UHc6Arp6TiqGynn",
            "QmYVF1azKfVM1pGWeBBEKSdMxj1AJbkDCvE3sxeVbXkDfT",
            "QmYyqzzAaZcuVK7T47jpWkcSeZMGT5h4nnvPeiwtaaryme",
            "QmYzdiKu8H789VoyM2XWQpkPhfvhRhCBht6cDMPink2wtc",
            "QmZat8MTAbx2xVbnY4xAAwF2YXowa17Ufrz8ySMV664dxA",
            "QmZAzxw1QEtqpoq96zjavfha1qKmYSAJpGCKuycHZLxSBD",
            "QmZdskRNYZaeMZBD74kQAZAgatyndvFoF9oDrukV3sL63T",
            "QmZE8o4qjA9nW2o52grpXyV8B2YB36mwwABkjNpGeoMhwE",
            "QmZevCdvnXgeUTjewP5H77Pmv3wWb3KDVoE3sL4pChtJ4h",
            "QmZFz98XN6epUCh7uh9KsAWBZw4h27hdqhwdp2oLfgADAM",
            "QmZGhEFGs2dBhwE1aAcA831d8nPYQZ6AkH3E1dcCWeWf86",
            "QmZo5JVQTyR9Ua5VcEXn2e47KKW99L6cYuj15zFWMya7dR",
            "QmZskzGFnR9u2isP8YCzuVQSe4g941RjF3gNeHVn2eUzgy",
            "QmZWwUgq6UYJLCgtPbzVdhThkPTNPHqezWiQKw7Lg3yqKV" };

            List<MappingPair> pairs = new List<MappingPair>();

            foreach (var tex in textures)
            {
                pairs.Add(new MappingPair() { file = tex + ".png", hash = tex });
            }

            var builder = new AssetBundleBuilder();

            builder.DownloadAndConvertAssets(pairs.ToArray());
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump All Wearables (Non bodies)")]
        public static void DumpAllNonBodiesWearables()
        {
            List<WearableItem> avatarItemList = GetAvatarMappingList("https://wearable-api.decentraland.org/v2/collections")
                .Where(x => x.category != WearableLiterals.Categories.BODY_SHAPE).ToList();

            List<MappingPair> contentList = ExtractMappingPairs(avatarItemList);

            var builder = new AssetBundleBuilder();

            UnityGLTF.GLTFImporter.OnGLTFWillLoad += GLTFImporter_OnGLTFWillLoad;
            builder.DownloadAndConvertAssets(contentList.ToArray(), (err) => { UnityGLTF.GLTFImporter.OnGLTFWillLoad -= GLTFImporter_OnGLTFWillLoad; });
        }

        private static void GLTFImporter_OnGLTFWillLoad(UnityGLTF.GLTFSceneImporter obj)
        {
            obj.importSkeleton = false;
            obj.maxTextureSize = 512;
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Zone -110,-110")]
        public static void DumpZoneArea()
        {
            var builder = new AssetBundleBuilder();
            builder.environment = ContentServerUtils.ApiEnvironment.ORG;
            builder.DumpArea(new Vector2Int(-110, -110), new Vector2Int(1, 1));
        }

        static void DumpAreaToMax(AssetBundleBuilder builder, int x, int y)
        {
            if (x >= 140 || y >= 140)
                return;

            Debug.Log($"--DumpAreaToMax {x}, {y}");
            int nextX = x + 10;
            int nextY = y;

            if (nextX > 130)
            {
                nextX = -130;
                nextY = y + 10;
            }

            builder.DumpArea(new Vector2Int(x, y), new Vector2Int(10, 10), (error) => DumpAreaToMax(builder, nextX, nextY));
        }


        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org 0,0")]
        public static void DumpCenterPlaza()
        {
            var builder = new AssetBundleBuilder();
            builder.skipAlreadyBuiltBundles = true;
            var zoneArray = Utils.GetCenteredZoneArray(new Vector2Int(0, 0), new Vector2Int(2, 2));
            builder.DumpArea(zoneArray);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Only Build Bundles")]
        public static void OnlyBuildBundles()
        {
            BuildPipeline.BuildAssetBundles(AssetBundleBuilderConfig.ASSET_BUNDLES_PATH_ROOT, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);
        }

        [System.Serializable]
        public class WearableItemArray
        {
            [System.Serializable]
            public class Collection
            {
                public string id;
                public List<WearableItem> wearables;
            }

            public List<Collection> data;
        }

        public static List<MappingPair> ExtractMappingPairs(List<WearableItem> wearableItems)
        {
            var result = new List<MappingPair>();

            foreach (var wearable in wearableItems)
            {
                foreach (var representation in wearable.representations)
                {
                    foreach (var datum in representation.contents)
                    {
                        result.Add(datum);
                    }
                }
            }

            return result;
        }


        public static List<WearableItem> GetAvatarMappingList(string url)
        {
            List<WearableItem> result = new List<WearableItem>();

            UnityWebRequest w = UnityWebRequest.Get(url);
            w.SendWebRequest();

            while (!w.isDone) { }

            if (!w.WebRequestSucceded())
            {
                Debug.LogWarning($"Request error! Parcels couldn't be fetched! -- {w.error}");
                return null;
            }

            var avatarApiData = JsonUtility.FromJson<WearableItemArray>("{\"data\":" + w.downloadHandler.text + "}");

            foreach (var collection in avatarApiData.data)
            {
                foreach (var wearable in collection.wearables)
                {
                    result.Add(wearable);
                }
            }

            return result;
        }
    }
}
