namespace UnityDropdown.Editor
{
    using System;
    using System.Collections.Generic;
    using SolidUtilities;

    // Part of the class, responsible solely for filling the tree with items. Only FillTreeWithItems method is used in
    // the main part of the class.
    public partial class DropdownMenu<T>
    {
        private void FillTreeWithItems(IList<DropdownItem<T>> items)
        {
            if (items == null)
                return;

            bool foundSelected = false;

            foreach (var item in items)
            {
                // DropdownMenu currently supports only a single selected value,
                // so if multiple items are marked as selected, only the first one will be shown as selected.
                if (item.IsSelected)
                {
                    if (foundSelected)
                    {
                        item.IsSelected = false;
                    }
                    else
                    {
                        foundSelected = true;
                    }
                }

                CreateDropdownItem(item);
            }
        }

        private void CreateDropdownItem(DropdownItem<T> item)
        {
            SplitFullItemPath(item.Path, out string folderPath, out string itemName);
            var directParentOfNewNode = folderPath.Length == 0 ? Root : CreateFoldersInPathIfNecessary(folderPath);
            directParentOfNewNode.AddChild(itemName, item);
        }

        private static void SplitFullItemPath(string nodePath, out string namespaceName, out string typeName)
        {
            int indexOfLastSeparator = nodePath.LastIndexOf('/');

            if (indexOfLastSeparator == -1)
            {
                namespaceName = string.Empty;
                typeName = nodePath;
            }
            else
            {
                namespaceName = nodePath.Substring(0, indexOfLastSeparator);
                typeName = nodePath.Substring(indexOfLastSeparator + 1);
            }
        }

        private DropdownNode<T> CreateFoldersInPathIfNecessary(string path)
        {
            var parentNode = Root;

            foreach (var folderName in path.AsSpan().Split('/'))
            {
                parentNode = parentNode.FindChild(folderName) ?? parentNode.AddChildFolder(folderName.ToString());
            }

            return parentNode;
        }
    }

    static class SpanExtensions
    {
        public static SplitSpanEnumerator Split(this ReadOnlySpan<char> span, char separator)
        {
            return new SplitSpanEnumerator(span, separator);
        }

        public ref struct SplitSpanEnumerator
        {
            private ReadOnlySpan<char> _span;
            private ReadOnlySpan<char> _currentSpan;
            private readonly char _separator;
            private int _currentIndex;

            public SplitSpanEnumerator GetEnumerator() => new (_span, _separator);

            public SplitSpanEnumerator(ReadOnlySpan<char> span, char separator)
            {
                _span = span;
                _separator = separator;
                _currentIndex = 0;
                _currentSpan = default;
            }

            public bool MoveNext()
            {
                while (_currentIndex < _span.Length && _span[_currentIndex] == _separator)
                {
                    _currentIndex++; // Skip leading separators
                }
                // If we reached the end of the span, return false
                if (_currentIndex >= _span.Length)
                {
                    return false;
                }
                // We known _span[_currentIndex] is not a separator here
                int nextIndex = _span[_currentIndex..].IndexOf(_separator);
                // If no separator is found, take the rest of the span
                if (nextIndex == -1)
                {
                    nextIndex = _span.Length - _currentIndex;
                }
                // Set the current span to the next segment
                _currentSpan = _span.Slice(_currentIndex, nextIndex);
                _currentIndex += nextIndex + 1; // Move past the separator
                return true;
            }

            public readonly ReadOnlySpan<char> Current => _currentSpan;
        }
    }
}
