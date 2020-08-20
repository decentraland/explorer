protoc --plugin=../../../node_modules/ts-protoc-gen/bin/protoc-gen-ts --js_out="import_style=commonjs,binary:." --ts_out="." ./engineinterface.proto
protoc --csharp_out=../../../../unity-client/Assets/Scripts/MainScripts/DCL/Models/Protocol --csharp_opt=base_namespace=DCL.Interface ./engineinterface.proto
