﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Drawing;
using System.Runtime.InteropServices;
using System;
using StringImageMaker;
using StringImageMaker.StringDrawing;

/*
  動的にDecalを作成するクラス。
  EmptyObjectに紐づけられる。prefabを一応つくるか

    クリックした点との衝突判定を通じて、Decalをはる
     */

public class DecalCasterController : MonoBehaviour {
    public GameObject decalPrefab_;
    private StringDrawer drawer_;

	// Use this for initialization
	void Start () {
        FontManager fontManager = new FontManager(
            new string[] { "meiryo UI" },
            minSize: 14,
            maxSize: 14
        );
        IMessageCreator messageCreator = RandomCharactorCreator.makeNumericCreator(minLen_: 3, maxLen_: 7);

        drawer_ = new StringDrawer(messageCreator, fontManager);
    }

    void makeObject(Vector3 pos)
    {
        // デバッグ用にいきなりDecalを作成する
        GameObject go = Instantiate(decalPrefab_) as GameObject;

        //go.transform.position = new Vector3(0, 1.37f, 0);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(1, 2, 1);

        // テクスチャ差し替え
        //Bitmap bitmap = new Bitmap(Application.dataPath + "/logo.jpg");
        Bitmap bitmap = drawer_.drawNext();
        //Bitmap bitmap = new Bitmap(width: 128, height: 128, format:System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
        g.Dispose();

        Texture2D texture = new Texture2D(bitmap.Width, bitmap.Height, TextureFormat.ARGB32, false);

        var bits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), 
            System.Drawing.Imaging.ImageLockMode.ReadOnly, 
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        var copyTo = new byte[bits.Width * bits.Height * 4];
        IntPtr head = bits.Scan0;
        
        unsafe {
            byte* src = (byte*)head.ToPointer();

            for (int r = 0; r < bits.Height; r++) {
                // Unityは下から上にデータが並んでいる
                int toOffset = (bits.Height - 1 - r) * bits.Width * 4;
                int fromOffset = r * bits.Width * 4;

                for (int c = 0; c < bits.Width; c++)
                {
                    int to_base = toOffset + c * 4;
                    int from_base = fromOffset + c * 4;

                    // coypTo = ARGB
                    // bits   = BGRA
                    copyTo[to_base]     = src[from_base + 3];
                    copyTo[to_base + 1] = src[from_base + 2];
                    copyTo[to_base + 2] = src[from_base + 1];
                    copyTo[to_base + 3] = src[from_base + 0];
                }
            }
        }

        bitmap.Dispose();
        texture.LoadRawTextureData(copyTo);
        texture.Apply();

        go.GetComponent<Decal>().m_Material = Instantiate(go.GetComponent<Decal>().m_Material);
        go.GetComponent<Decal>().m_Material.mainTexture = texture;
        //go.GetComponent<Decal>().GetComponent<Renderer>().material.mainTexture = texture;
    }

    // Update is called once per frame
    void Update () {
		if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 hitpos = hit.point;
                makeObject(hitpos);
            }
        }
    }

}