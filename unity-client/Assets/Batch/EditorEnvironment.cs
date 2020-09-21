﻿using System.Collections.Generic;

namespace DCL
{
    public class EditorEnvironment
    {
        public readonly IDirectory directory;
        public readonly IFile file;
        public readonly IAssetDatabase assetDatabase;
        public readonly IWebRequest webRequest;
        public readonly IBuildPipeline buildPipeline;

        public EditorEnvironment(IDirectory directory, IFile file, IAssetDatabase assetDatabase, IWebRequest webRequest, IBuildPipeline buildPipeline)
        {
            this.directory = directory;
            this.file = file;
            this.assetDatabase = assetDatabase;
            this.webRequest = webRequest;
            this.buildPipeline = buildPipeline;
        }

        public static EditorEnvironment CreateWithDefaultImplementations()
        {
            return new EditorEnvironment
            (
                directory: new SystemWrappers.Directory(),
                file: new SystemWrappers.File(),
                assetDatabase: new UnityEditorWrappers.AssetDatabase(),
                webRequest: new UnityEditorWrappers.WebRequest_Editor(),
                buildPipeline: new UnityEditorWrappers.BuildPipeline()
            );
        }

        public static EditorEnvironment CreateWithMockImplementations()
        {
            return new EditorEnvironment
            (
                directory: new MockWrappers.Directory(),
                file: new MockWrappers.File(),
                assetDatabase: new MockWrappers.AssetDatabase(),
                webRequest: new MockWrappers.WebRequest(),
                buildPipeline: new UnityEditorWrappers.BuildPipeline()
            );
        }
    }
}