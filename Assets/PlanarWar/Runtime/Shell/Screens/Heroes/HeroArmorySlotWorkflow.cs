using PlanarWar.Client.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlanarWar.Client.UI.Screens.Heroes
{
    public static class HeroArmorySlotWorkflow
    {
        private static readonly IReadOnlyDictionary<string, string> StatLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["str"] = "Str",
            ["dex"] = "Dex",
            ["con"] = "Con",
            ["int"] = "Int",
            ["wis"] = "Wis",
            ["cha"] = "Cha",
            ["hp"] = "HP",
            ["mp"] = "MP",
            ["ac"] = "AC",
        };

        private static readonly string[] OrderedSlots =
        {
            "head",
            "chest",
            "legs",
            "feet",
            "hands",
            "mainhand",
            "offhand",
            "ring1",
            "ring2",
            "neck",
        };

        public static IReadOnlyList<string> StandardSlots { get; } = new ReadOnlyCollection<string>(OrderedSlots);

        public static string NormalizeSlot(string slot)
        {
            if (string.IsNullOrWhiteSpace(slot)) return string.Empty;
            return slot.Trim()
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(" ", string.Empty)
                .ToLowerInvariant();
        }

        public static bool IsStandardSlot(string slot)
        {
            var normalized = NormalizeSlot(slot);
            return !string.IsNullOrWhiteSpace(normalized)
                && OrderedSlots.Any(standard => string.Equals(standard, normalized, StringComparison.OrdinalIgnoreCase));
        }

        public static bool SlotsMatch(string candidateSlot, string selectedSlot)
        {
            var normalizedCandidate = NormalizeSlot(candidateSlot);
            var normalizedSelected = NormalizeSlot(selectedSlot);
            return !string.IsNullOrWhiteSpace(normalizedCandidate)
                && !string.IsNullOrWhiteSpace(normalizedSelected)
                && string.Equals(normalizedCandidate, normalizedSelected, StringComparison.OrdinalIgnoreCase);
        }

        public static List<HeroArmoryItemSnapshot> GetCompatibleArmoryItems(HeroArmoryBridgeSnapshot armory, string selectedSlot)
        {
            if (armory?.ArmoryItems == null || string.IsNullOrWhiteSpace(selectedSlot)) return new List<HeroArmoryItemSnapshot>();

            return armory.ArmoryItems
                .Where(item => item != null
                    && item.SlotIndex.HasValue
                    && SlotsMatch(item.Template?.Slot, selectedSlot))
                .OrderBy(item => item.SlotIndex.Value)
                .ThenBy(item => DescribeItemName(item.Template, item.ItemId), StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static HeroEquipmentEntrySnapshot FindEquippedEntry(HeroEquipmentSnapshot equipment, string selectedSlot)
        {
            if (equipment?.Equipment == null || string.IsNullOrWhiteSpace(selectedSlot)) return null;
            return equipment.Equipment.FirstOrDefault(entry => entry != null && SlotsMatch(entry.Slot, selectedSlot));
        }

        public static bool HasEquippedSlot(HeroEquipmentSnapshot equipment, string selectedSlot)
        {
            return FindEquippedEntry(equipment, selectedSlot) != null;
        }

        public static string FormatSlotLabel(string slot)
        {
            return NormalizeSlot(slot) switch
            {
                "mainhand" => "Main Hand",
                "offhand" => "Off Hand",
                "ring1" => "Ring 1",
                "ring2" => "Ring 2",
                "head" => "Head",
                "chest" => "Chest",
                "legs" => "Legs",
                "feet" => "Feet",
                "hands" => "Hands",
                "neck" => "Neck",
                _ => string.IsNullOrWhiteSpace(slot) ? "Slot" : slot.Trim(),
            };
        }

        public static string BuildSlotSurfaceTitle(bool isOperative)
        {
            return isOperative ? "Operative kit slots" : "Hero gear slots";
        }

        public static string BuildSelectedSlotCurrentText(HeroEquipmentSnapshot equipment, string selectedSlot, bool isOperative)
        {
            var slotLabel = FormatSlotLabel(selectedSlot);
            var noun = EquipmentNounLower(isOperative);
            var actor = ActorNounLower(isOperative);

            if (equipment == null)
            {
                return $"No equipped-slot truth surfaced for this {actor}.";
            }

            var equipped = FindEquippedEntry(equipment, selectedSlot);
            if (equipped == null)
            {
                return $"Current {slotLabel}: empty {noun} slot.";
            }

            var stats = FormatStats(equipped.Template);
            var itemName = DescribeItemName(equipped.Template, equipped.ItemId);
            return string.IsNullOrWhiteSpace(stats)
                ? $"Current {slotLabel}: {itemName}."
                : $"Current {slotLabel}: {itemName} • {stats}.";
        }

        public static string BuildCompatibleItemSummary(IEnumerable<HeroArmoryItemSnapshot> compatibleItems, string selectedSlot, bool isOperative)
        {
            var items = compatibleItems?.Where(item => item != null).ToList() ?? new List<HeroArmoryItemSnapshot>();
            var slotLabel = FormatSlotLabel(selectedSlot);
            var noun = EquipmentNounLower(isOperative);

            if (items.Count == 0)
            {
                return $"No compatible {slotLabel} {noun} is available in the shared armory.";
            }

            return $"{items.Count} compatible {slotLabel} {noun} option{Plural(items.Count)} available from shared armory truth.";
        }

        public static string BuildArmoryItemChoice(HeroArmoryItemSnapshot item, bool isOperative)
        {
            return BuildArmoryItemChoice(item, isOperative, null);
        }

        public static string BuildArmoryItemChoice(HeroArmoryItemSnapshot item, bool isOperative, HeroEquipmentEntrySnapshot equippedEntry)
        {
            if (item == null) return string.Empty;
            var slotLabel = FormatSlotLabel(item.Template?.Slot);
            var name = DescribeItemName(item.Template, item.ItemId);
            var stats = FormatStats(item.Template);
            var quantity = item.Qty ?? 1;
            var statText = string.IsNullOrWhiteSpace(stats) ? string.Empty : $" • {stats}";
            var equippedText = IsSameEquippedItem(item, equippedEntry) ? " • already equipped" : string.Empty;
            return $"{name} x{quantity} • {slotLabel}{statText}{equippedText}";
        }

        public static string BuildEquipButtonText(HeroArmoryItemSnapshot selectedItem, string actorName, string selectedSlot, bool isOperative)
        {
            return BuildEquipButtonText(selectedItem, null, actorName, selectedSlot, isOperative);
        }

        public static string BuildEquipButtonText(HeroArmoryItemSnapshot selectedItem, HeroEquipmentEntrySnapshot equippedEntry, string actorName, string selectedSlot, bool isOperative)
        {
            var noun = EquipmentNounLower(isOperative);
            var actor = ActorNounLower(isOperative);

            if (string.IsNullOrWhiteSpace(actorName))
            {
                return $"Select {actor} for {noun}";
            }

            if (selectedItem == null)
            {
                return $"Select compatible {FormatSlotLabel(selectedSlot)} {noun}";
            }

            var itemName = DescribeItemName(selectedItem.Template, selectedItem.ItemId);
            if (IsSameEquippedItem(selectedItem, equippedEntry))
            {
                return $"{itemName} already equipped";
            }

            return $"Equip {itemName} to {actorName.Trim()}";
        }

        public static bool IsSameEquippedItem(HeroArmoryItemSnapshot item, HeroEquipmentEntrySnapshot equippedEntry)
        {
            if (item == null || equippedEntry == null) return false;
            if (!SlotsMatch(item.Template?.Slot, equippedEntry.Slot)) return false;

            if (SameNonBlank(item.GenesisId, equippedEntry.GenesisId)) return true;
            if (SameNonBlank(item.ItemId, equippedEntry.ItemId)) return true;
            if (SameNonBlank(item.Template?.Id, equippedEntry.Template?.Id)) return true;

            var itemName = DescribeItemName(item.Template, item.ItemId);
            var equippedName = DescribeItemName(equippedEntry.Template, equippedEntry.ItemId);
            return SameNonBlank(itemName, equippedName);
        }

        public static string BuildNoArmoryChoiceText(string selectedSlot, bool isOperative)
        {
            return $"No compatible {FormatSlotLabel(selectedSlot)} {EquipmentNounLower(isOperative)}";
        }

        public static string EquipmentNounLower(bool isOperative)
        {
            return isOperative ? "kit" : "gear";
        }

        public static string EquipmentNounTitle(bool isOperative)
        {
            return isOperative ? "Kit" : "Gear";
        }

        public static string ActorNounLower(bool isOperative)
        {
            return isOperative ? "operative" : "hero";
        }

        public static string DescribeItemName(HeroEquipmentTemplateSnapshot template, string fallbackItemId)
        {
            return FirstNonBlank(template?.Name, template?.Id, fallbackItemId, "item");
        }

        public static string FormatStats(HeroEquipmentTemplateSnapshot template)
        {
            if (template?.Stats == null || template.Stats.Count == 0) return string.Empty;
            return string.Join(", ", template.Stats
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Key))
                .Take(4)
                .Select(pair => $"{FormatStatLabel(pair.Key)} {FormatSignedStatValue(pair.Value)}"));
        }

        private static string FormatStatLabel(string key)
        {
            var normalized = key?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized)) return "Stat";

            var parts = normalized
                .Replace('-', '_')
                .Replace(' ', '_')
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return "Stat";

            return string.Join(" ", parts.Select(part =>
            {
                if (StatLabels.TryGetValue(part, out var label)) return label;
                return part.Length == 1
                    ? part.ToUpperInvariant()
                    : char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant();
            }));
        }

        private static string FormatSignedStatValue(double value)
        {
            if (value > 0) return $"+{value:0.##}";
            if (value < 0) return $"{value:0.##}";
            return "0";
        }

        private static string FirstNonBlank(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
            }
            return string.Empty;
        }

        private static bool SameNonBlank(string left, string right)
        {
            return !string.IsNullOrWhiteSpace(left)
                && !string.IsNullOrWhiteSpace(right)
                && string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string Plural(int count)
        {
            return count == 1 ? string.Empty : "s";
        }
    }
}
