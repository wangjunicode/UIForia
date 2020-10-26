using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace SeedLib {
    public interface ISearchResult {
        ImageLocator Icon { get; }
        string Text { get; }
    }
    
    public class SearchResult : ISearchResult {
        public ImageLocator Icon { get; set; }
        public string Text { get; set; }
    }

    [Template("SeedLib/SearchBox/SearchBox.xml")]
    public class SearchBox : UIElement {
        public string searchTerm;
        public string placeholder;
        public ISearchResult selectedSearchResult;
        public List<ISearchResult> searchResults;

        private UIElement searchBoxInput;
        private UIInputElement textField;
        
        public void ClearSearchTerm() {
            searchTerm = string.Empty;
            selectedSearchResult = null;
        }

        public bool HasSearchResults() {
            return searchResults != null && searchResults.Count > 0;
        }
        
        public override void OnCreate() {
            searchBoxInput = FindById("search-box-input");
            textField = FindById<UIInputElement>("text-field");
            textField.onFocus += TextFieldOnFocus;
            textField.onBlur += TextFieldOnBlur;
        }
        
        public override void OnDestroy() {
            if (textField != null) {
                textField.onFocus -= TextFieldOnFocus;
                textField.onBlur -= TextFieldOnBlur;
            }
        }

        private void TextFieldOnFocus(FocusEvent obj) {
            searchBoxInput.SetAttribute("focus-within", "true");
        }
        
        private void TextFieldOnBlur(BlurEvent obj) {
            searchBoxInput.SetAttribute("focus-within", null);
        }

        public void OnSelectedSearchResult(ISearchResult selected) {
            selectedSearchResult = selected;
        }
    }
}