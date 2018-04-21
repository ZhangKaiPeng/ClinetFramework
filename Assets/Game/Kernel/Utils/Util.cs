using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using LitJson;

public static class Util
{
	public const int DEFAULT_HTTP_TIMEOUT = 10000;

	private static string kPersistentDataPath = null;

	public const string kDefalutGreyMatName = "UIGrey";

	public static Material DefalutUIGreyMat;

	public static void Init()
	{		
		InitUIUtil();
		InitRandomNameConfig();
	}

	#region random name

	private static JsonData _randomNameConfig;
	private static void InitRandomNameConfig()
	{
		try{			
			//UnityEngine.Object configObj = ResourceManager.Instance.GetLoadedAsset(AssetBundleConst.kConfigRandomname, "randomName.bytes");
			//TextAsset textObj = configObj as TextAsset;
			//_randomNameConfig = JsonMapper.ToObject(textObj.text);
		}catch(System.Exception e)
		{
			DYLogger.LogError(e);
		}
	}

	public static string GetRandomName()
	{
		string result = "";
		try{
			JsonData surnameArray = _randomNameConfig["surname"];
			JsonData firstNameArray = _randomNameConfig["firstName"];
			JsonData firstNameRatio = _randomNameConfig["firstNameRatio"];

			int firstNameRandomCount = 0;
			float firstNameRandomValue = Random.Range(0, 1.0f);
			double nextRatio = 0;
			for(int i = 0, count = firstNameRatio.Count; i < count; i++)
			{				
				nextRatio += (double)(firstNameRatio[i]["ratio"]);
				if(firstNameRandomValue <= nextRatio)
				{
					firstNameRandomCount = (int)(firstNameRatio[i]["randomCount"]);
					break;
				}
			}

			if(0 != firstNameRandomValue)
			{
				string surname = surnameArray[Random.Range(0, surnameArray.Count)].ToString();
				string firstName = "";

				for(int i = 0; i < firstNameRandomCount; i++)
				{
					firstName += firstNameArray[Random.Range(0, firstNameArray.Count)].ToString();
				}

				result = surname + firstName;
			}else
			{
				DYLogger.LogError("get firstName random count err! check the ratio please");
			}

		}catch(System.Exception e){DYLogger.LogError(e);}

		return result;
	}

	#endregion

	#region net

	/// <summary>
	/// 获取一个www文件
	/// </summary>
	public static IEnumerator GetWWWFile(string file, System.Action<float> processHandle, System.Action<WWW> completeHandle)
	{
		WWW www = new WWW(file);

		while (false == www.isDone)
		{
			if (null != processHandle)
			{
				processHandle(www.progress);
			}

			yield return null;
		}            

		if (null != completeHandle)
		{
			completeHandle(www);
		}

		yield return null;
	}

	public static HttpWebResponse CreateGetHttpResponse(string url, int timeout = 0, int range = 0)
	{
		if (string.IsNullOrEmpty (url)) 
		{
			return null;	
		}

		HttpWebRequest request = WebRequest.Create (url) as HttpWebRequest;

		if(null == request) return null;

		request.Method = "GET";

		if (0 == timeout)
			timeout = DEFAULT_HTTP_TIMEOUT;

		if (timeout > 0)
			request.Timeout = timeout;

		if (range > 0)
			request.AddRange (range);

		return request.GetResponse () as HttpWebResponse;
	}

	#endregion

	#region time

	public static int GetLoclUnixTimeStamp()
	{
		return (int)(System.DateTime.Now - GetUnixBaseDateTime()).TotalSeconds;
	}

	public static System.DateTime GetUnixBaseDateTime()
	{        
		return System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
	}

	public static System.DateTime GetUnixDateTime(long unixTimeStamp)
	{        
		System.TimeSpan tempTimeSpan = new System.TimeSpan(unixTimeStamp * 10000000);
		System.DateTime tempDateTime = Util.GetUnixBaseDateTime().Add(tempTimeSpan);
		return tempDateTime;
	}

	/// <summary>
	/// Gets the format time.
	/// </summary>
	public static string GetFormatTime(int second)
	{
		int sec = second % 60;
		int min = (int)(second / 60) % 60;
		int hour = second / 3600;
		return string.Format("{0:00}:{1:00}:{2:00}",hour,min,sec);
	}

	public static string GetFormatTimeForCounting(int second, bool d, bool h, bool m, bool s)
	{
		string result = string.Empty;
		
		int sec = second % 60;
		int min = (int)(second / 60) % 60;
		int hour = (second / 3600) % 24;
		int day = second / 86400;

		return result;
	}

	#endregion

	#region json

	public static string GetJsonData(JsonData jsonData, string key, string defaultValue)
	{
		if(null == jsonData) return defaultValue;

		if(jsonData.Keys.Contains(key)) return (string)jsonData[key];
		else return defaultValue;
	}

	public static bool GetJsonData(JsonData jsonData, string key, bool defaultValue)
	{
		if(null == jsonData) return defaultValue;

		if(jsonData.Keys.Contains(key)) return (bool)jsonData[key];
		else return defaultValue;
	}

	public static int GetJsonData(JsonData jsonData, string key, int defaultValue)
	{
		if(null == jsonData) return defaultValue;

		if(jsonData.Keys.Contains(key)) return (int)jsonData[key];
		else return defaultValue;
	}

	public static float GetJsonData(JsonData jsonData, string key, float defaultValue)
	{
		if(null == jsonData) return defaultValue;

		if(jsonData.Keys.Contains(key))
		{
			float ret = defaultValue;
			if(float.TryParse(jsonData[key].ToJson(), out ret)) return ret;
		}
		 
		return defaultValue;
	}

	#endregion

	public static string GetPlatformString()
	{
		string ret = "unknow";
		#if UNITY_STANDALONE_WIN
		ret = "win";
		#endif
		#if UNITY_STANDALONE_OSX
		ret = "macosx";
		#endif
		#if UNITY_STANDALONE_LINUX
		ret = "linux";
		#endif
		#if UNITY_WEBPLAYER
		ret = "web";
		#endif
		#if UNITY_IPHONE || UNITY_IOS
		ret = "iOS";
		#endif
		#if UNITY_ANDROID
		ret = "Android";
		#endif
		#if UNITY_METRO
		ret = "metro";
		#endif
		#if UNITY_WP8
		ret = "wp8";
		#endif
		#if UNITY_WEBGL
		ret = "webgl";
		#endif
		return ret;
	}

	public static int GetVersionValue(string versionStr)
	{
		return GetVersionValue(versionStr, 3);
	}

	/// <summary>
	/// 获取infomation中记录的版本号的数值大小
	/// unitLength : 版本号以.分割的单个单位长度, 如0.0.100单位长度为3
	/// </summary>
	public static int GetVersionValue(string versionStr, int unitLength)
	{
		int tempVersionValue = 0;
		if (string.IsNullOrEmpty(versionStr) == false)
		{
			try
			{

				string[] versionValues = versionStr.Split('.');                
				int tempValue;
				for(int i = 0, length = versionValues.Length; i<length ; i++)
				{
					tempValue = int.Parse(versionValues[i]);
					if(0 != tempValue) tempValue = tempValue * (int)Mathf.Pow(10, (length - 1 - i) * unitLength);
					tempVersionValue += tempValue;
				}

			}catch(System.Exception e)
			{
				DYLogger.LogErrorFormat("Parse cache version failed!! Version text: {0}\nerr={1}", versionStr, e);
			}
		}        
		return tempVersionValue;
	}

	#region path

	public static string GetStreamingAssetsFilePath(string filePath)
	{
		string result = Application.streamingAssetsPath;

		result = string.Format("{0}/asset/{1}/{2}", result, Util.GetPlatformString(), filePath);

		#if (UNITY_ANDROID || UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_EDITOR

		#elif UNITY_STANDALONE_WIN
		result = "file:///" + result;
		#else
		result = "file://" + result;
		#endif

		return result;
	}

	public static string GetDownloadedFilePath(string relativeFilePath)
	{
		if(string.IsNullOrEmpty(kPersistentDataPath)) kPersistentDataPath = Application.persistentDataPath;
		return string.Format("{0}/assets/{1}/{2}", kPersistentDataPath, Util.GetPlatformString(), relativeFilePath);
	}

	/// <summary>
	/// 1.1返回缓存文件路径 
	/// 1.2缓存不存在,返回包内文件路径
	/// 2.Web端返回远程文件路径
	/// </summary>
	public static string GetAssetBundleFilePath(string bundleName)
	{
		string bundlePath = GetDownloadedFilePath (bundleName);
		string bundleURL = null;

		// check if we have download the file
		#if !UNITY_WEBPLAYER && !UNITY_WEBGL
		bool fileExists = false;
		try {
			fileExists = File.Exists(bundlePath);
		}
		catch (System.Exception e)
		{
			Debug.LogException(e);
		}

		if (fileExists)
		{
		#if UNITY_STANDALONE_WIN
			bundleURL = "file:///" + bundlePath;
		#else
			bundleURL = "file://" + bundlePath;
		#endif
		}
		else 
		{
			// this file not in download floder try to load from bundle
			bundlePath = GetStreamingAssetsFilePath(bundleName);

			bundleURL = bundlePath;
		}
		#else

		bundlePath = string.Format("StreamingAssets/asset/{0}/{1}", Util.GetPlatformString(), bundleName);
		bundleURL = Util.GetCurrentWebDomain() + bundlePath;

		#endif

		return bundleURL;
	}   

	public static string GetCurrentWebDomain()
	{
		#if UNITY_EDITOR
		return string.Format("file://{0}/", Application.dataPath);
		#else
		return Application.absoluteURL;
		#endif
	}

	#endregion

	#region security

	/// <summary>
	/// HashToMD5Hex
	/// </summary>
	public static string HashToMD5Hex(string sourceStr) 
	{
		byte[] Bytes = Encoding.UTF8.GetBytes(sourceStr);
		using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider()) {
			byte[] result = md5.ComputeHash(Bytes);
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < result.Length; i++)
				builder.Append(result[i].ToString("x2"));
			return builder.ToString();
		}
	}

	/// <summary>
	/// 计算字符串的MD5值
	/// </summary>
	public static string md5(string source) 
	{
		MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
		byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
		byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
		md5.Clear();

		string destString = "";
		for (int i = 0; i < md5Data.Length; i++) 
		{
			destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
		}
		destString = destString.PadLeft(32, '0');
		return destString;
	}

	/// <summary>
	/// 计算文件的MD5值
	/// </summary>
	public static string md5file(string file) 
	{
		try 
		{
			FileStream fs = new FileStream(file, FileMode.Open);
			System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] retVal = md5.ComputeHash(fs);
			fs.Close();

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < retVal.Length; i++) 
			{
				sb.Append(retVal[i].ToString("x2"));
			}
			return sb.ToString();
		} catch (System.Exception ex) {
			throw new System.Exception("md5file() fail, error:" + ex.Message);
		}
	}

	public static byte[] SHA1Hash(byte[] source)
	{
		HashAlgorithm iSHA = new SHA1CryptoServiceProvider ();
		return iSHA.ComputeHash (source);
	}

	public static string SHA1HashString(byte[] source)
	{
		byte[] StrRes = SHA1Hash (source);
		StringBuilder EnText = new StringBuilder();
		foreach (byte iByte in StrRes)
		{
			EnText.AppendFormat("{0:x2}", iByte);
		}
		return EnText.ToString();
	}

	#endregion

	#region io

	public static byte[] DecompressData(byte[] source, int offset, int size)
	{
		//		BZip2InputStream gzi = new BZip2InputStream (new MemoryStream (source, offset, size)); 
		GZipInputStream gzi = new GZipInputStream (new MemoryStream (source, offset, size));
		MemoryStream output = new MemoryStream ();
		int count = 0;
		byte[] data = new byte[1024];
		while ((count = gzi.Read(data, 0, data.Length)) != 0) {
			output.Write(data, 0, count);
		}
		gzi.Close ();
		byte[] result = output.ToArray();
		output.Close ();
		return result;
	}

	public static byte[] CompressData(byte[] source, int offset, int size)
	{
		MemoryStream output = new MemoryStream ();
		//		BZip2OutputStream gzo = new BZip2OutputStream (output);
		GZipOutputStream gzo = new GZipOutputStream (output);
		gzo.SetLevel (9);
		gzo.Write (source, offset, size);
		gzo.Close ();
		byte [] result = output.ToArray ();
		output.Close ();
		return result;
	}

	/// <summary>
	/// 解压缩文件(保持目录结构
	/// </summary>
	/// <param name="sourceFile">源文件</param>
	/// <param name="targetPath">目标路经</param>
	public static bool Decompress(string sourceFile, string targetPath)
	{
		if (!File.Exists(sourceFile))
		{
			Debug.LogError(string.Format("未能找到文件 '{0}' ", sourceFile));
		}

		CreateDir(targetPath);

		using (ZipInputStream s = new ZipInputStream(File.OpenRead(sourceFile)))
		{
			ZipEntry theEntry;
			while ((theEntry = s.GetNextEntry()) != null)
			{
				string directorName = Path.Combine(targetPath, Path.GetDirectoryName(theEntry.Name));
				string fileName = Path.Combine(directorName, Path.GetFileName(theEntry.Name));
				// 创建目录
				if (directorName.Length > 0)
				{
					if(false == Directory.Exists(directorName)) Directory.CreateDirectory(directorName);
				}
				if (fileName != string.Empty)
				{
					if(File.Exists(fileName)) File.Delete(fileName);
					using (FileStream streamWriter = File.Create(fileName))
					{
						int size = 4096;
						byte[] data = new byte[ 4 * 1024];
						while (true)
						{
							size = s.Read(data, 0, data.Length);
							if (size > 0)
							{
								streamWriter.Write(data, 0, size);
							}
							else break;
						}
					}
				}
			}
		}
		return true;
	}

	public static void CreateDir(string path, bool recursive = true)
	{
		if(recursive)
		{
			string tempDir = path;
			while(true)
			{
				tempDir = Path.GetDirectoryName(tempDir);

				if(string.IsNullOrEmpty(tempDir)) break;

				if(Directory.Exists(tempDir))
				{
					break;
				}else
				{
					Directory.CreateDirectory(tempDir);
				}
			}
		}

		if(false == Directory.Exists(path)) Directory.CreateDirectory(path);
	}

	/// <summary>
	/// 复制文件或者文件夹到目标路径下
	/// </summary>
	/// <param name="from">源路径</param>
	/// <param name="to">目标路径</param>
	/// <param name="filter">排除包含字符串</param>
	public static void CopyDirectory(string from, string to, string filter = "")
	{
		try
		{			
			if (to[to.Length - 1] != Path.DirectorySeparatorChar)
			{
				to += Path.DirectorySeparatorChar;
			}

			if (!Directory.Exists(to))
			{
				Directory.CreateDirectory(to);
			}

			string[] fileList = Directory.GetFileSystemEntries(from);
			foreach (string file in fileList)
			{
				if(false == string.IsNullOrEmpty(filter))
				{
					if(file.Contains(filter)) continue;
				}

				if (Directory.Exists(file))
				{
					CopyDirectory(file, to + Path.GetFileName(file));
				}
				else
				{
					File.Copy(file, to + Path.GetFileName(file), true);
				}
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogError("拷贝文件夹出错"+ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static void CopyFile(string src, string to, bool recursive)
	{
		try
		{
			Util.CreateDir(Path.GetDirectoryName(to), true);

			File.Copy(src, to);

		}catch(System.Exception e)
		{
			Debug.LogError(e);
		}
	}	

	#endregion

	#region ui

	private static void InitUIUtil()
	{
		//DefalutUIGreyMat = ResourceManager.Instance.GetLoadedAsset(AssetBundleConst.kCommon, kDefalutGreyMatName) as Material;
		//if(null == DefalutUIGreyMat) DYLogger.LogError("get DefalutUIGreyMat fail!");
	}

	public static CanvasGroup SetWidgetAlpha(GameObject widgetGO,float alpha)
	{
		CanvasGroup tempCanvasGroup = widgetGO.GetComponent<CanvasGroup>();

		if (null == tempCanvasGroup)
			tempCanvasGroup = widgetGO.AddComponent<CanvasGroup>();

		tempCanvasGroup.alpha = alpha;
		return tempCanvasGroup;
	}

	public static Canvas SetWidgetRenderOrder(GameObject widgetGO, int order, string sortingLayerName)
	{		
		Canvas result = null;
		result = GetComponent<Canvas>(widgetGO);
		GetComponent<GraphicRaycaster>(widgetGO);

		result.overrideSorting = true;
		result.sortingOrder = order;
		result.sortingLayerName = sortingLayerName;

		return result;
	}

	public static Canvas SetWidgetRenderOrder(GameObject widgetGO, int order)
	{
		return SetWidgetRenderOrder(widgetGO, order, "Default");
	}

	public static void SetParticleAlpha(GameObject particleGO,float alpha)
	{
		ParticleSystem[] tempPSArray = particleGO.GetComponentsInChildren<ParticleSystem>();
		Color tempColor;
		ParticleSystem.MinMaxGradient tempGradient;
		ParticleSystem.MainModule tempParticleModule;
		if (null != tempPSArray && tempPSArray.Length > 0)
		{
			for(int i = tempPSArray.Length - 1 ; i >= 0 ; i--)
			{
				tempParticleModule = tempPSArray[i].main;
				tempGradient = tempParticleModule.startColor;
				tempColor = tempGradient.color;
				tempColor.a = alpha;
				tempGradient.color = tempColor;
				tempParticleModule.startColor = tempGradient;
			}
		}
	}

	public static void SetUIWidgetGrey(GameObject[] widgetGOArray, bool grey,bool includeChild,bool ignoreText)
	{
		if (null == widgetGOArray || widgetGOArray.Length == 0)
		{
			return;
		}

		int i;
		for(i = widgetGOArray.Length-1;i>=0;i--)
		{
			SetUIWidgetGrey(widgetGOArray[i],grey,includeChild,ignoreText);
		}

	}

	public static void SetUIWidgetGrey(GameObject widgetGO, bool grey,bool includeChild,bool ignoreText)
	{
		if (null == widgetGO)
			return;

		Graphic tempGraphic;
		tempGraphic = widgetGO.GetComponent<Graphic>();
		if (null != tempGraphic)
		{            
			if (false == ignoreText
				|| false == tempGraphic is Text)
			{
				UIWdgetGreyItem tempGreyItem = widgetGO.GetComponent<UIWdgetGreyItem>();
				if (null != tempGreyItem)
				{
					tempGreyItem.SetWidgetGrey(grey);
				}
				else
				{
					tempGraphic.material = grey ? DefalutUIGreyMat : null;
				}                
			}
		}

		if (true == includeChild)
		{
			foreach(Transform child in widgetGO.transform)
			{
				SetUIWidgetGrey(child.gameObject,grey,includeChild,ignoreText);
			}
		}
	}

	public static Rect GetWorldSpaceRect(RectTransform rectTrans)
	{
		Vector3 worldPosZero = rectTrans.TransformPoint(Vector3.zero);
		float scaleFactorX = (rectTrans.TransformPoint(Vector3.right) - worldPosZero).magnitude;
		float scaleFactorY = (rectTrans.TransformPoint(Vector3.up) - worldPosZero).magnitude;

		//DYDYLogger.Log(scaleFactorX  + " " + scaleFactorY + " " + rectTrans.position);

		Vector2 tempRectSize = new Vector2(rectTrans.rect.width * scaleFactorX,rectTrans.rect.height* scaleFactorY);
		Vector2 tempRectPos = new Vector2(rectTrans.position.x - tempRectSize.x * rectTrans.pivot.x , rectTrans.position.y - tempRectSize.y * rectTrans.pivot.y);

		Rect tempRect = new Rect(tempRectPos.x,tempRectPos.y,tempRectSize.x,tempRectSize.y);

		return tempRect;
	}

	/// <summary>
	/// 获取target,在origin所在坐标系中的相对坐标(返回anchoredPosition)
	/// </summary>
	public static Vector3 GetRelativeRectTransPoint(RectTransform origin, RectTransform target)
	{
		/*
		GameObject tempGO = new GameObject();
		RectTransform tempRectTrans = tempGO.AddComponent<RectTransform>();

		tempRectTrans.pivot = origin.pivot;
		tempRectTrans.anchorMax = origin.anchorMax;
		tempRectTrans.anchorMin = origin.anchorMin;

		tempRectTrans.SetParent(target,false);
		tempRectTrans.anchoredPosition3D = Vector3.zero;

		tempRectTrans.SetParent(origin.parent,true);
		Vector3 result = tempRectTrans.anchoredPosition3D;
		GameObject.Destroy(tempGO);
		return result;
		*/
		return GetRelativeRectTransData(origin, target)[0];
	}

	public static Vector3[] GetRelativeRectTransData(RectTransform origin, RectTransform target)
	{
		GameObject tempGO = new GameObject();
		RectTransform tempRectTrans = tempGO.AddComponent<RectTransform>();

		tempRectTrans.pivot = origin.pivot;
		tempRectTrans.anchorMax = origin.anchorMax;
		tempRectTrans.anchorMin = origin.anchorMin;

		tempRectTrans.SetParent(target,false);
		tempRectTrans.anchoredPosition3D = Vector3.zero;

		tempRectTrans.SetParent(origin.parent,true);
		Vector3[] result = new Vector3[2];
		result[0] = tempRectTrans.anchoredPosition3D;
		result[1] = tempRectTrans.localScale;
		GameObject.Destroy(tempGO);
		return result;
	}

	/// <summary>
	/// 获取世界坐标worldPos在origin坐标系中的anchoredPosition
	/// </summary>
	public static Vector3 GetRelativeRectTransPoint(RectTransform origin,Vector3 worldPos)
	{
		GameObject tempGO = new GameObject();
		RectTransform tempRectTrans = tempGO.AddComponent<RectTransform>();

		tempRectTrans.pivot = origin.pivot;
		tempRectTrans.anchorMax = origin.anchorMax;
		tempRectTrans.anchorMin = origin.anchorMin;

		tempRectTrans.position = worldPos;
		tempRectTrans.SetParent(origin.parent,true);

		Vector3 result = tempRectTrans.anchoredPosition3D;
		GameObject.Destroy(tempGO);
		return result;
	}

	public static RectTransform GetEmptyRectTrans()
	{
		GameObject temp = new GameObject();
		return temp.AddComponent<RectTransform>();
	}

	public static RectTransform GetEmptyRectTrans(RectTransform originRectTrasn)
	{
		RectTransform tempRectTrans = GetEmptyRectTrans();

		tempRectTrans.SetParent(originRectTrasn.parent, false);

		tempRectTrans.pivot = originRectTrasn.pivot;
		tempRectTrans.anchorMax = originRectTrasn.anchorMax;
		tempRectTrans.anchorMin = originRectTrasn.anchorMin;
		tempRectTrans.anchoredPosition3D = originRectTrasn.anchoredPosition3D;
		tempRectTrans.sizeDelta = originRectTrasn.sizeDelta;

		return tempRectTrans;
	}

	/// <summary>
	/// 有两个不同摄像机下的物体A,B,获取屏幕中显示的B在A中的世界坐标
	/// </summary>
	public static Vector3 GetRelativeWorldPoint(Vector3 worldPosA,Camera camA,Vector3 WorldPosB,Camera camB)
	{
		Vector3 screenPosA = camA.WorldToScreenPoint(worldPosA);
		Vector3 screenPosB = camB.WorldToScreenPoint(WorldPosB);

		screenPosB.z = screenPosA.z;
		return camA.ScreenToWorldPoint(screenPosB);
	}

	/// <summary>
	/// 给定一个摄像机,获取在该摄像机的渲染方向上,以baseedWorldPos为原点
	/// (即假设该点所在的摄像机的视口垂直横切面上的物体scale比例为1)
	/// ,一世界坐标为offsetObjPos的物体的scale比例
	/// </summary>
	public static float GetCameraScaleRatioBaseOnOffset(Camera cam,Vector3 basedWorldPos,Vector3 offsetObjPos)
	{
		float ratio = 1;
		if (cam.orthographic == false)
		{
			float a = Mathf.Abs((cam.transform.position - basedWorldPos).z);
			float b = (basedWorldPos - offsetObjPos).z;
			ratio = (a - b) / a;
		}
		return ratio;
	}

	/// <summary>
	/// 将parent移动至child所在的父节点中以及对应位置中,然后再将child的父节点设为parent,并保持位置不变
	/// </summary>
	public static void SetParentBaseOn2Cam(Transform child,Transform parent,Camera childCam,Camera parentCam)
	{
		Vector3 relativePos  = GetRelativeWorldPoint(child.position,childCam,parent.position,parentCam);
		parent.SetParent(child.parent, false);
		parent.position = relativePos;

		child.SetParent(parent,false);
		child.localPosition = Vector3.zero;          
	}

	public static void SetGameObjectLayer(GameObject go, int layerIndex, bool includeChild)
	{        
		if(go.layer != layerIndex) go.layer = layerIndex;

		if (includeChild)
		{
			Transform parent = go.transform;
			foreach(Transform child in parent)
			{
				SetGameObjectLayer(child.gameObject,layerIndex,includeChild);
			}
		}
	}

	public static void SetGameObjectSortingLayer(GameObject go, int layerId, bool ignoreHigherLayer)
	{
		SetGameObjectSortingLayer(go, layerId, ignoreHigherLayer, false, 0, true);
	}

	public static void SetGameObjectSortingLayer(GameObject go, int layerId, bool ignoreHigherLayer, bool setSortingOrder, int sortingOrder, bool raycastTarget)
	{
		if(null == go) return;

		Canvas tempCanvas;

		bool setCanvas = true;

		if(ignoreHigherLayer)
		{
			setCanvas = false;

			tempCanvas = go.GetComponentInParent<Canvas>();

			if(null == tempCanvas) 
			{				
				setCanvas = true;
			}
			else
			{
				int destLayerValue = SortingLayer.GetLayerValueFromID(layerId);
				int srcLayerValue = SortingLayer.GetLayerValueFromID(tempCanvas.sortingLayerID);

				if(destLayerValue > srcLayerValue || (setSortingOrder && sortingOrder > tempCanvas.sortingOrder))
				{					
					setCanvas = true;
				}
			}
		}

		if(setCanvas)
		{
			tempCanvas = Util.GetComponent<Canvas>(go);

			tempCanvas.overrideSorting = true;
			tempCanvas.sortingLayerID = layerId;
			if(setSortingOrder) tempCanvas.sortingOrder = sortingOrder;
		}

		GraphicRaycaster gr = Util.GetComponent<GraphicRaycaster>(go, raycastTarget);
		if(null != gr) gr.enabled = raycastTarget;

		ParticleSystemRenderer[] tempParticleRenders = go.GetComponentsInChildren<ParticleSystemRenderer>(true);

		ParticleSystemRenderer tempParticleRender;
		if(null != tempParticleRenders && tempParticleRenders.Length > 0)
		{
			int srcLayerValue;
			int destLayerValue = SortingLayer.GetLayerValueFromID(layerId);

			for(int i = tempParticleRenders.Length-1; i>=0; i--)
			{
				tempParticleRender = tempParticleRenders[i];

				if(ignoreHigherLayer)
				{
					srcLayerValue = SortingLayer.GetLayerValueFromID(tempParticleRender.sortingLayerID);
					if(srcLayerValue >= destLayerValue) continue;
				}

				tempParticleRender.sortingLayerID = layerId;
				if(setSortingOrder && (false == ignoreHigherLayer || tempParticleRender.sortingOrder < sortingOrder)) tempParticleRender.sortingOrder = sortingOrder;
			}
		}
	}

	public static int GetDropdownOptionIndex(string itemStr,Dropdown dropDownWidget)
	{
		int result = 0;

		foreach(Dropdown.OptionData tempOptionData in dropDownWidget.options)
		{
			if(itemStr == tempOptionData.text)
			{
				break;
			}
			result++;
		}

		if (result == dropDownWidget.options.Count)
		{
			DYLogger.LogError("找不到对应的option: " + itemStr);
			result = dropDownWidget.value;
		}
		return result;
	}

	public static T GetComponent<T>(GameObject go, bool autoAdd = true) where T : Component
	{
		T com = null;

		if (null != go)
		{
			com = go.GetComponent<T>();

			if (autoAdd && null == com)
			{
				com = go.AddComponent<T>();
			}
		}            

		return com;
	}

	public static Camera TryGetCam(Transform trans)
	{
		Camera tempCam = null;

		if (null != trans)
		{            
			Canvas tempCanvas = trans.GetComponentInParent<Canvas>();
			if (null != tempCanvas)
			{
				tempCam = tempCanvas.worldCamera;
			}
			if (null == tempCam)
			{
				tempCam = trans.GetComponentInParent<Camera>();
			}
		}

		if(null == tempCam) tempCam = Camera.main;

		return tempCam;
	}

	public static GameObject Instantiate(GameObject originObj, Transform parent)
	{
		return Instantiate(originObj, parent, false);
	}

	public static GameObject Instantiate(GameObject originObj, Transform parent, bool worldPosStay)
	{
		GameObject tempGO = GameObject.Instantiate(originObj);
		tempGO.transform.SetParent(parent, worldPosStay);
		return tempGO;
	}

	public static T Instantiate<T>(GameObject originObj, Transform parent) where T : Component
	{
		T result = null;

		if(null != originObj)
		{
			GameObject tempGO = Instantiate(originObj, parent);
			result = tempGO.GetComponent<T>();
		}else
		{
			DYLogger.LogError("Util.Instantiate fail, prefab cannot be null");
		}

		if(null == result)
		{
			DYLogger.LogError("Util.Instantiate fail, get component null, prefabName=" + originObj.name);
		}
		return result;
	}	

	public static void ClearChild(Transform root)
	{		
		if (root == null) return;
		for (int i = root.childCount - 1; i >= 0; i--)
		{
			GameObject.Destroy(root.GetChild(i).gameObject);
		}
	}

	public static Vector3 GetPointerPos()
	{
		if(Input.touchCount > 0) return Input.GetTouch(Input.touchCount-1).position;
		return Input.mousePosition;
	}

	public static bool GetPointerDown()
	{
		if(Input.touchCount > 0)
		{
			return Input.GetTouch(Input.touchCount - 1).phase == TouchPhase.Began;
		}
		return Input.GetMouseButtonDown(0);
	}

	public static bool GetPointer()
	{
		if(Input.touchCount > 0)
		{
			TouchPhase tp = Input.GetTouch(Input.touchCount - 1).phase;
			return tp == TouchPhase.Moved || tp == TouchPhase.Stationary;
		}
		return Input.GetMouseButton(0);
	}

	public static bool GetPointerUp()
	{
		if(Input.touchCount > 0)
		{
			TouchPhase tp = Input.GetTouch(Input.touchCount - 1).phase;
			return tp == TouchPhase.Canceled || tp == TouchPhase.Ended;
		}
		return Input.GetMouseButtonUp(0);
	}

	#endregion

	public static float GetAnimLength(Animator animator)
	{
		float animationLength = 0;
		if(null != animator && null != animator.runtimeAnimatorController)
		{
			AnimationClip[] tempAnimClips = animator.runtimeAnimatorController.animationClips;
			for(int i = tempAnimClips.Length-1; i>=0; i--)
			{ 
//				animationLength += tempAnimClips[i].length;
				animationLength = Mathf.Max(animationLength, tempAnimClips[i].length);
			}
		}
		return animationLength;
	}

	public static EffectElement CreateEffect(GameObject prefab, Transform parent)
	{
		EffectElement effect;
		GameObject effectGO = Util.Instantiate(prefab, parent);
		effect = Util.GetComponent<EffectElement>(effectGO);
		return effect;
	}

	#region local storage

	public static void SetInt(string key, int value)
	{
		SetInt(key, value, true);
	}

	public static void SetInt(string key, int value, bool playerLimited)
	{
		PlayerPrefs.SetInt(GetLocalStorageKey(key, playerLimited), value);
	}

	public static int GetInt(string key, int defaultValue)
	{
		return GetInt(key, defaultValue, true);
	}

	public static int GetInt(string key, int defaultValue, bool playerLimited)
	{
		return PlayerPrefs.GetInt(GetLocalStorageKey(key, playerLimited), defaultValue);
	}

	public static void SetString(string key, string value)
	{
		SetString(key, value, true);
	}

	public static void SetString(string key, string value, bool playerLimited)
	{
		PlayerPrefs.SetString(GetLocalStorageKey(key, playerLimited), value);
	}

	public static string GetString(string key, string defaultValue)
	{
		return GetString(key, defaultValue, true);
	}

	public static string GetString(string key, string defaultValue, bool playerLimited)
	{
		return PlayerPrefs.GetString(GetLocalStorageKey(key, playerLimited), defaultValue);
	}

	public static bool HasKey(string key, bool playerLimited)
	{
		return PlayerPrefs.HasKey(GetLocalStorageKey(key, playerLimited));
	}

	private static string GetLocalStorageKey(string sublKey, bool playerLimited)
	{
		return string.Empty;
	}

	#endregion

	#region 拷贝Text到剪贴板

	#if UNITY_IOS
	[DllImport ("__Internal")]
	private static extern void _copyTextToClipboard(string text);

	[DllImport ("__Internal")]
	private static extern void _openAppStoreURL(string url);

	#endif

	public static void SetSystemCopyBuffer(string content)
	{
		if(string.IsNullOrEmpty(content)) return;
		try
		{
			#if UNITY_ANDROID
			AndroidJavaClass tempAndroidClass = new AndroidJavaClass("com.util.copybuffer.ClipboardTools");
			tempAndroidClass.CallStatic("copyTextToClipboard", content);
			#elif UNITY_IOS && !UNITY_EDITOR
			_copyTextToClipboard(content);
			#else
			GUIUtility.systemCopyBuffer = content;
			#endif

		}catch(System.Exception e){DYLogger.LogException(e);}
	}

	public static string GetSystemCopyBuffer()
	{
		string result = "";
		try
		{
			#if UNITY_ANDROID
			AndroidJavaClass tempAndroidClass = new AndroidJavaClass("com.util.copybuffer.ClipboardTools");
			result = tempAndroidClass.CallStatic<string>("getTextFromClipboard");
			#elif UNITY_IOS

			#else
			result = GUIUtility.systemCopyBuffer;
			#endif

		}catch(System.Exception e){DYLogger.LogException(e);}
		return result;
	}

	public static void OpenAppStoreURL(string url,System.Action callback)
	{
		#if UNITY_IOS
		_openAppStoreURL(url);
		callback();
		#endif
	}

	#endregion

	public static string GetFormatFileSize(int size)
	{
		string result = "";

		if (size < 1024)
		{
			result = string.Format("{0}B", size);
			return result;        
		}

		float tempSize = (float)size / 1024;

		if (tempSize < 1024)
		{
			tempSize = Mathf.Round(tempSize * 100) / 100.0f;
			result = string.Format("{0}KB", tempSize);
			return result;
		}

		tempSize = tempSize / 1024;

		if (tempSize < 1024)
		{
			tempSize = Mathf.Round(tempSize * 100) / 100.0f;
			result = string.Format("{0}MB", tempSize);
			return result;
		}

		tempSize = tempSize / 1024;
		tempSize = Mathf.Round(tempSize * 100) / 100.0f;
		result = string.Format("{0}GB", tempSize);

		return result;
	}

	public static List<int> ConvertList(List<long> srcList, bool ensureNotNull)
	{
		if(null == srcList) return ensureNotNull ? new List<int>(0) : null;
		List<int> result = new List<int>(srcList.Count);
		for(int i =0, count = srcList.Count; i<count; i++)
		{			
			result.Add((int)srcList[i]);
		}

		return result;
	}

	public static List<long> ConvertList(List<int> srcList, bool ensureNotNull)
	{
		if(null == srcList) return ensureNotNull ? new List<long>(0) : null;
		List<long> result = new List<long>(srcList.Count);
		for(int i =0, count = srcList.Count; i<count; i++)
		{			
			result.Add((long)srcList[i]);
		}

		return result;
	}	

	/// <summary>
	/// 手机震动
	/// </summary>
	public static void Vibrate() 
	{
		#if UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1

		Handheld.Vibrate ();

		#endif
	}

	public static Texture2D CaptureScreenshot()
	{
		GameObject[] camGOs = GameObject.FindGameObjectsWithTag("Camera");
		List<Camera> cams = null;
		if(null != camGOs && camGOs.Length > 0)
		{
			cams = new List<Camera>(camGOs.Length);
			Camera tempCam;
			for(int i = 0, count = camGOs.Length; i<count; i++)
			{
				tempCam = camGOs[i].GetComponent<Camera>();
				if(null != tempCam) cams.Add(tempCam);
			} 
		}
		return CaptureScreenshot(cams);
	}

	public static Texture2D CaptureScreenshot(List<Camera> cams)
	{
		if(null == cams || cams.Count == 0)
		{
			DYLogger.LogError("CaptureScreenshot fail!, render camera cannot be null!");
			return null;
		}
		
		RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 16);

		RenderTexture oriRt = RenderTexture.active;

		Camera tempCam;

		cams.Sort(delegate(Camera x, Camera y) {
			if(x == null || y == null) return 0;
			return x.depth.CompareTo(y.depth);
		});

		RenderTexture[] cachedOriginRTs = new RenderTexture[cams.Count];
		for(int i = 0, count = cams.Count; i <count; i++)
		{
			tempCam = cams[i];
			if(null == tempCam) continue;

			cachedOriginRTs[i] = tempCam.targetTexture;
			tempCam.targetTexture = rt;
			tempCam.Render();
		}

		RenderTexture.active = rt;

		Texture2D t = new Texture2D(Screen.width, Screen.height);
		t.filterMode = FilterMode.Bilinear;
		t.anisoLevel = 1;
		t.wrapMode = TextureWrapMode.Clamp;
		t.ReadPixels(new Rect(0,0, Screen.width, Screen.height), 0, 0);
		t.Apply();

		for(int i = 0, count = cams.Count; i <count; i++)
		{
			tempCam = cams[i];
			if(null == tempCam) continue;

			tempCam.targetTexture = cachedOriginRTs[i];
		}

		RenderTexture.active = oriRt;

		return t;
	}
}
