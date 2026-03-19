namespace DingFrame
{
	using System.IO;
	using UnityEngine;
	using UnityEditor.AssetImporters;

	[ScriptedImporter(1, "pbdata")]
	public class PBDataImporter : ScriptedImporter 
	{
		public override void OnImportAsset(AssetImportContext ctx)
		{
			TextAsset asset = new (File.ReadAllBytes(ctx.assetPath));
			ctx.AddObjectToAsset("main", asset);
			ctx.SetMainObject(asset);
		}
	}
}
