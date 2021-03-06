using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class TowerManager : MonoBehaviour
{
	public const string upgradeLocked = "PATH LOCKED", maxUpgrade = "MAXED UPGRADE";
	public static TowerManager Instance { get; private set; }

	public Sprite padlock;
	public Sprite defTex;
	public Transform parent;
	public Image placeImg;
	public TMP_Text placePrice;
	public GameObject prefab;
	public GameObject upgradePanel;

	[Header("No Peeking")]
	public TowerData[] towerData;
	public List<Tower> towers;
	public Tower placingTower;
	public Tower selectedTower;
	public bool deselecting;

	public int towerId;

	private void Start()
	{
		Instance = this;
		LoadAll();
	}

	public void Spawn() =>
		Spawn(towerId);

	public void Spawn(int id) =>
		StartCoroutine(CoSpawn(id));

	private IEnumerator CoSpawn(int id)
	{
		if (GameManager.Instance.money < towerData[id].price)
			yield break;
		yield return new WaitForFixedUpdate();
		placingTower = Instantiate(prefab, parent).GetComponent<Tower>();
		placingTower.data = towerData[id];
		towers.Add(placingTower);
	}

	public void Next() =>
		Choose((towerId + 1) % towerData.Length);

	public void Choose(int id)
	{
		towerId = id;
		placePrice.text = $"${towerData[id].price}";
		placeImg.sprite = towerData[id].icon ? towerData[id].icon : defTex;
	}

	public void Upgrade(int path)
	{
		selectedTower.Upgrade((Path)path);
		UpdatePaths();
	}

	public void ReSelect()
	{
		if (selectedTower)
			Select(selectedTower);
	}

	public void Select(Tower tower)
	{
		deselecting = false;
		DeselectInternal();

		selectedTower = tower;
		selectedTower.transform.Find("View").GetComponent<Renderer>().enabled = true;
		upgradePanel.SetActive(true);
		UpdatePaths();
	}

	public void DeSelect()
	{
		deselecting = true;
		StartCoroutine(CoDeSelect());
	}

	public void Sell()
	{
		GameManager.Instance.money += selectedTower.sellPrice;
		Destroy(selectedTower.gameObject);
		DeselectInternal();
	}

	private void DeselectInternal()
	{
		if (selectedTower)
			selectedTower.transform.Find("View").GetComponent<Renderer>().enabled = false;

		selectedTower = null;
		upgradePanel.SetActive(false);
	}

	private IEnumerator CoDeSelect()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForFixedUpdate();

		if (!deselecting)
			yield break;

		DeselectInternal();
	}

	public void UpdatePaths()
	{
		upgradePanel.transform.Find("Tower").GetComponent<TMP_Text>().text = selectedTower.data.name;
		upgradePanel.transform.Find("Tier").GetComponent<TMP_Text>().text = $"{(int)selectedTower.path1Tier}-{(int)selectedTower.path2Tier}-{(int)selectedTower.path3Tier}";

		var sellTxt = upgradePanel.transform.Find("Sell").Find("Text (TMP)").GetComponent<TMP_Text>();
		sellTxt.text = $"Sell {selectedTower.sellPrice}";

		var p1Button = upgradePanel.transform.Find("Path (1)");
		var p2Button = upgradePanel.transform.Find("Path (2)");
		var p3Button = upgradePanel.transform.Find("Path (3)");

		var pi1 =  p1Button.Find("Sprite").GetComponent<Image>();
		var pi2 =  p2Button.Find("Sprite").GetComponent<Image>();
		var pi3 =  p3Button.Find("Sprite").GetComponent<Image>();

		var pt1 = p1Button.Find("Text (TMP)").GetComponent<TMP_Text>();
		var pt2 = p2Button.Find("Text (TMP)").GetComponent<TMP_Text>();
		var pt3 = p3Button.Find("Text (TMP)").GetComponent<TMP_Text>();

		var pp1 = p1Button.Find("Path Price (1)").GetComponent<TMP_Text>();
		var pp2 = p2Button.Find("Path Price (2)").GetComponent<TMP_Text>();
		var pp3 = p3Button.Find("Path Price (3)").GetComponent<TMP_Text>();

		switch (selectedTower.disPath)
		{
		case Path.None:
			var s1 = selectedTower.UpgradeSprite(Path.Path1);
			var s2 = selectedTower.UpgradeSprite(Path.Path2);
			var s3 = selectedTower.UpgradeSprite(Path.Path3);
			pi1.sprite = s1 ? s1 : defTex;
			pi2.sprite = s2 ? s2 : defTex;
			pi3.sprite = s3 ? s3 : defTex;

			pt1.text = selectedTower.UpgradeName(Path.Path1) ?? maxUpgrade;
			pt2.text = selectedTower.UpgradeName(Path.Path2) ?? maxUpgrade;
			pt3.text = selectedTower.UpgradeName(Path.Path3) ?? maxUpgrade;

			pp1.text = $"${selectedTower.UpgradePrice(Path.Path1)}";
			pp2.text = $"${selectedTower.UpgradePrice(Path.Path2)}";
			pp3.text = $"${selectedTower.UpgradePrice(Path.Path3)}";
			break;
		case Path.Path1:
			pi1.sprite = padlock;
			pt1.text = upgradeLocked;
			pp1.text = "";

			switch (selectedTower.primPath)
			{
			case Path.Path2:
				UpdatePath(Path.Path2, Path.Path3, selectedTower.path3Tier, pt2, pp2, pi2, pt3, pp3, pi3);
				break;
			case Path.Path3:
				UpdatePath(Path.Path3, Path.Path2, selectedTower.path2Tier, pt3, pp3, pi3, pt2, pp2, pi2);
				break;

			default:
				UpdatePathNormal(Path.Path2, Path.Path3, pt2, pp2, pi2, pt3, pp3, pi3);
				break;
			}
			break;
		case Path.Path2:
			pi2.sprite = padlock;
			pt2.text = upgradeLocked;
			pp2.text = "";

			switch (selectedTower.primPath)
			{
			case Path.Path1:
				UpdatePath(Path.Path1, Path.Path3, selectedTower.path3Tier, pt1, pp1, pi1, pt3, pp3, pi3);
				break;
			case Path.Path3:
				UpdatePath(Path.Path3, Path.Path1, selectedTower.path1Tier, pt3, pp3, pi3, pt1, pp1, pi1);
				break;

			default:
				UpdatePathNormal(Path.Path1, Path.Path3, pt1, pp1, pi1, pt3, pp3, pi2);
				break;
			}
			break;
		case Path.Path3:
			pi3.sprite = padlock;
			pt3.text = upgradeLocked;
			pp3.text = "";

			switch (selectedTower.primPath)
			{
			case Path.Path1:
				UpdatePath(Path.Path1, Path.Path2, selectedTower.path2Tier, pt1, pp1, pi1, pt2, pp2, pi2);
				break;
			case Path.Path2:
				UpdatePath(Path.Path2, Path.Path1, selectedTower.path1Tier, pt2, pp2, pi2, pt1, pp1, pi1);
				break;

			default:
				UpdatePathNormal(Path.Path1, Path.Path2, pt1, pp1, pi1, pt2, pp2, pi2);
				break;
			}
			break;
		}
	}

	private void UpdatePathNormal(Path a, Path b,
		TMP_Text pta, TMP_Text ppa, Image pia,
			TMP_Text ptb, TMP_Text ppb, Image pib)
	{
		var sa = selectedTower.UpgradeSprite(a);
		var sb = selectedTower.UpgradeSprite(b);
		pia.sprite = sa ? sa : defTex;
		pib.sprite = sb ? sb : defTex;

		pta.text = selectedTower.UpgradeName(a) ?? maxUpgrade;
		ptb.text = selectedTower.UpgradeName(b) ?? maxUpgrade;

		ppa.text = $"${selectedTower.UpgradePrice(a)}";
		ppb.text = $"${selectedTower.UpgradePrice(b)}";
	}

	private void UpdatePath(Path prim, Path sec, Tier secTier,
		TMP_Text pt1, TMP_Text pp1, Image pi1,
			TMP_Text pt2, TMP_Text pp2, Image pi2)
	{
		var s1 = selectedTower.UpgradeSprite(prim);
		pi1.sprite = s1 ? s1 : defTex;
		pt1.text = selectedTower.UpgradeName(prim) ?? maxUpgrade;
		pp1.text = $"${selectedTower.UpgradePrice(prim)}";

		if (secTier < Tier.Tier2)
		{
			var s2 = selectedTower.UpgradeSprite(sec);
			pi2.sprite = s2 ? s2 : defTex;
			pt2.text = selectedTower.UpgradeName(sec);
			pp2.text = $"${selectedTower.UpgradePrice(sec)}";
			return;
		}

		var ls2 = selectedTower.LastUpgradeSprite(sec);
		pi2.sprite = ls2 ? ls2 : defTex;
		pt2.text = maxUpgrade;
		pp2.text = "";
	}
}
