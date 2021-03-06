using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Tower
{
	public int sellPrice { get => Mathf.RoundToInt(invested * 0.8f); }

	public Path primPath;
	public Path disPath;
	public Tier path1Tier;
	public Tier path2Tier;
	public Tier path3Tier;

	public void Upgrade(Upgrade upgrade)
	{
		dartProps |= upgrade.props;
		stick += upgrade.stick;
		explosion += upgrade.explosion;
		dartSpeed += upgrade.dartSpeed;
		reload += upgrade.reload;
		kb += upgrade.kb;
		dps += upgrade.dps;
		blast += upgrade.blast;
		damage += upgrade.damage;
		pierce += upgrade.pierce;
		range += upgrade.range;
	}

	public Dart Fire(Ant ant)
	{
		Vector2 dir = ant.transform.position - transform.position;
		dir.Normalize();
		transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

		var obj = Instantiate(dartPrefab);
		obj.transform.position = new Vector3(transform.position.x, transform.position.y, obj.transform.position.z);
		obj.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

		var dart = obj.GetComponent<Dart>();
		dart.kb = kb;
		dart.dps = dps;
		dart.dir = dir;
		dart.pierce = pierce;
		dart.damage = damage;
		dart.props = dartProps;
		dart.timeout = range;
		dart.blast = blast;
		dart.speed = dartSpeed;
		dart.explosion = explosion;
		dart.stick = stick;

		return dart;
	}

	public void Upgrade(Path path)
	{
		switch (path)
		{
		case Path.Path1:
			if (UpgradePath(path, ref path1Tier, path2Tier, path3Tier, Path.Path2, Path.Path3))
				Upgrade(data.path1[(int)path1Tier - 1]);
			break;
		case Path.Path2:
			if (UpgradePath(path, ref path2Tier, path1Tier, path3Tier, Path.Path1, Path.Path3))
				Upgrade(data.path2[(int)path2Tier - 1]);
			break;
		case Path.Path3:
			if (UpgradePath(path, ref path3Tier, path1Tier, path2Tier, Path.Path1, Path.Path2))
				Upgrade(data.path3[(int)path3Tier - 1]);
			break;
		}
	}

	private bool UpgradePath(Path path, ref Tier pathTier, Tier tier1, Tier tier2, Path path1, Path path2)
	{
		if (GameManager.Instance.money < UpgradePrice(path, pathTier))
			return false;

		if (disPath == path)
			return false;

		if (primPath != Path.None && primPath != path && pathTier >= Tier.Tier2)
			return false;

		if (pathTier >= Tier.Tier5)
			return false;

		pathTier++;
		if (primPath == Path.None)
		{
			if (pathTier >= Tier.Tier3)
				primPath = path;
		}

		if (tier1 > Tier.Tier0)
			disPath = path2;
		if (tier2 > Tier.Tier0)
			disPath = path1;

		int price = UpgradePrice(path, pathTier - 1);
		invested += price;
		GameManager.Instance.money -= price;
		return true;
	}

	public string UpgradeName(Path path, Tier tier) =>
		path switch
		{
			Path.Path1 => tier < Tier.Tier5 ? data.path1[(int)tier].name : null,
			Path.Path2 => tier < Tier.Tier5 ? data.path2[(int)tier].name : null,
			Path.Path3 => tier < Tier.Tier5 ? data.path3[(int)tier].name : null,
			_ => null,
		};

	public string UpgradeName(Path path) =>
		path switch
		{
			Path.Path1 => path1Tier < Tier.Tier5 ? data.path1[(int)path1Tier].name : null,
			Path.Path2 => path2Tier < Tier.Tier5 ? data.path2[(int)path2Tier].name : null,
			Path.Path3 => path3Tier < Tier.Tier5 ? data.path3[(int)path3Tier].name : null,
			_ => null,
		};

	public Sprite UpgradeSprite(Path path, Tier tier) =>
		path switch
		{
			Path.Path1 => data.path1[(int)tier].sprite,
			Path.Path2 => data.path2[(int)tier].sprite,
			Path.Path3 => data.path3[(int)tier].sprite,
			_ => null,
		};

	public Sprite UpgradeSprite(Path path) =>
		path switch
		{
			Path.Path1 => data.path1[(int)path1Tier].sprite,
			Path.Path2 => data.path2[(int)path2Tier].sprite,
			Path.Path3 => data.path3[(int)path3Tier].sprite,
			_ => null,
		};

	public Sprite LastUpgradeSprite(Path path) =>
		path switch
		{
			Path.Path1 => path1Tier > Tier.Tier0 ? UpgradeSprite(path, path1Tier - 1) : null,
			Path.Path2 => path1Tier > Tier.Tier0 ? UpgradeSprite(path, path2Tier - 1) : null,
			Path.Path3 => path1Tier > Tier.Tier0 ? UpgradeSprite(path, path3Tier - 1) : null,
			_ => null,
		};

	public int UpgradePrice(Path path, Tier tier) =>
		path switch
		{
			Path.Path1 => data.path1[(int)tier].price,
			Path.Path2 => data.path2[(int)tier].price,
			Path.Path3 => data.path3[(int)tier].price,
			_ => 0,
		};

	public int UpgradePrice(Path path) =>
		path switch
		{
			Path.Path1 => data.path1[(int)path1Tier].price,
			Path.Path2 => data.path2[(int)path2Tier].price,
			Path.Path3 => data.path3[(int)path3Tier].price,
			_ => 0,
		};
}
