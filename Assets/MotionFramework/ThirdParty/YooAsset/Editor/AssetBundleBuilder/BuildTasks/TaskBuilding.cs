﻿using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	public class TaskBuilding : IBuildTask
	{
		public class UnityManifestContext : IContextObject
		{
			public AssetBundleManifest UnityManifest;
		}

		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();

			Debug.Log($"开始构建......");
			BuildAssetBundleOptions opt = buildParametersContext.GetPipelineBuildOptions();
			AssetBundleManifest unityManifest = BuildPipeline.BuildAssetBundles(buildParametersContext.PipelineOutputDirectory, buildMapContext.GetPipelineBuilds(), opt, buildParametersContext.Parameters.BuildTarget);
			if (unityManifest == null)
				throw new Exception("构建过程中发生错误！");

			UnityManifestContext unityManifestContext = new UnityManifestContext();
			unityManifestContext.UnityManifest = unityManifest;
			context.SetContextObject(unityManifestContext);

			// 拷贝原生文件
			if (buildParametersContext.Parameters.DryRunBuild == false)
			{
				CopyRawBundle(buildMapContext, buildParametersContext);
			}
		}

		/// <summary>
		/// 拷贝原生文件
		/// </summary>
		private void CopyRawBundle(BuildMapContext buildMapContext, AssetBundleBuilder.BuildParametersContext buildParametersContext)
		{
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				if (bundleInfo.IsRawFile)
				{
					string dest = $"{buildParametersContext.PipelineOutputDirectory}/{bundleInfo.BundleName}";
					foreach (var buildAsset in bundleInfo.BuildinAssets)
					{
						if (buildAsset.IsRawAsset)
							EditorTools.CopyFile(buildAsset.AssetPath, dest, true);
					}
				}
			}
		}
	}
}