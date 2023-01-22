﻿using System.Diagnostics;

namespace BTSET1_Converter
{
	internal class BTSET1 : BTSET
	{
		// This tool is also meant for analysis, so it's also keeping track of who is accessing fields.
		internal UInt16 BattleEntryTableOffset;
		internal override Dictionary<UInt16, Bonuses> BonusesTable { get; set; }
		internal override Dictionary<UInt16, ModelEntry> ModelTable { get; set; }
        internal override Dictionary<UInt16, Placement[]> PlacementTable { get; set; } //84 sets of 8 placements
        private List<UInt16> BattleEntryOffsets = new();
		internal readonly Dictionary<UInt16, BattleEntry> Battles1 = new();
		internal readonly Dictionary<UInt16, AutoBattleEntry> AutoBattles = new();
		internal readonly Dictionary<UInt16, BattleEntry> Battles2 = new();
		protected override byte[] Data { get; set; }
		protected override bool[] Coverage { get; set; } // set to true if the byte has been read at least once. If all true => full coverage
        private bool IsFullCoverage => Coverage.All(w => w == true);
		private IEnumerable<UInt16> UnusedBattles => Battles1.Keys.Concat(AutoBattles.Keys).Concat(Battles2.Keys).Where(w => !BattleEntryOffsets.Contains(w));
		private IEnumerable<BattleEntry> InvalidBattles => Battles1.Values.Concat(Battles2.Values).Where(w => w.Id == 0xFFFF);
		private IEnumerable<AutoBattleEntry> InvalidAutoBattles => AutoBattles.Values.Where(w => w.Id == 0xFFFF);

		internal BTSET1()
		{
			BonusesTable = new();
			ModelTable = new();
			PlacementTable = new();
		}
		public async Task Parse(string filePath)
		{
			Data = await File.ReadAllBytesAsync(filePath);
			if (Data.Length > UInt16.MaxValue) throw new InvalidDataException("file too big!");
			Coverage = new bool[Data.Length];
			UInt16 curOffset = 0;
			BattleEntryTableOffset = ReadUInt16(ref curOffset);
			for (int i = 0; i < 15; i++)
			{
				UInt16 off = curOffset;
				Bonuses entry = new(ref curOffset, this);
				BonusesTable.Add(off, entry);
			}
			for (int i = 0; i < 188; i++)
			{
				UInt16 off = curOffset;
				ModelEntry entry = new(ref curOffset, this);
				ModelTable.Add(off, entry);
			}
			for (int i = 0; i < 84; i++)
			{
				UInt16 off = curOffset;
				Placement[] placements = new Placement[8];
				for (int j = 0; j < 8; j++)
				{
					Placement entry = new(ref curOffset, this);
					placements[j] = entry;
				}
				PlacementTable.Add(off, placements);
			}
			Debug.Assert(curOffset == BattleEntryTableOffset);
			for (int i = 0; i < 376; i++)
			{
				UInt16 offset = ReadUInt16(ref curOffset);
				BattleEntryOffsets.Add(offset);
			}
			BattleEntry curBTEntry;
			do
			{
				UInt16 off = curOffset;
				BattleEntry entry = new(ref curOffset, this, true);
				Battles1.Add(off, entry);
				curBTEntry = entry;
			} while (curBTEntry.Id != 0xFFFF);
			AutoBattleEntry curAutoBTEntry;
			do
			{
				UInt16 off = curOffset;
				AutoBattleEntry entry = new(ref curOffset, this);
				AutoBattles.Add(off, entry);
				curAutoBTEntry = entry;
			} while (curAutoBTEntry.Id != 0xFFFF);
			for (int i = 0; i < 363; i++)
			{
				UInt16 off = curOffset;
				BattleEntry entry = new(ref curOffset, this, false);
				Battles2.Add(off, entry);
			}
			Debug.Assert(IsFullCoverage);
		}
	}
}