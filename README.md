# AJBAvalonia Controls

This project contains a collection of custom controls for Avalonia. Below is a list of the controls included in this project along with their public members.

## Controls

### CheckComboBoxEx
A combo box control that supports selecting multiple items via checkboxes and displays a summary or the selected items.
- **Properties**:
  - `ClearButtonBackground`: Background brush for the clear button.
  - `DisplayMemberBinding`: Binding used to display item members.
  - `DropGlyphPath`: Path geometry used for the drop glyph icon.
  - `DropGlyphSize`: Size used for the drop glyph icon.
  - `GlyphType`: Glyph type used for the drop icon.
  - `HorizontalContentAlignment`: Horizontal alignment of the content.
  - `IsDropDownOpen`: Indicates whether the drop-down is open.
  - `ItemsSource`: Source collection for items in the control.
  - `ItemTemplate`: Template used to display each item.
  - `MaxDropDownHeight`: Maximum height for the dropdown list.
  - `MaxSelectedItemsBeforeSummaryText`: Threshold of selected items before showing summary text.
  - `PlaceholderFontStyle`: Font style used for placeholder text.
  - `PlaceholderForeground`: Brush used for placeholder text foreground.
  - `PlaceholderText`: Placeholder text shown when no selection exists.
  - `SelectedCount`: Number of selected items.
  - `SelectedItemBorderBackground`: Background brush for selected item borders.
  - `SelectedItemBorderBrush`: Brush used for selected item borders.
  - `SelectedItemBorderCornerRadius`: Corner radius for selected item borders.
  - `SelectedItemBorderThickness`: Thickness of the selected item border.
  - `SelectedItemMargin`: Margin applied to selected item visuals.
  - `SelectedItems`: Collection of selected items.
  - `SelectedItemsDisplay`: How selected items are displayed (individual items or summary text).
  - `SelectedItemsTemplate`: Template used to render selected items in the header area.
  - `SelectionMode`: Selection mode used in the dropdown list.
  - `ShowClearButton`: Indicates whether the clear button is shown.
  - `SummaryText`: Summary text displayed when summary mode is used.
  - `SummaryTextFormatString`: Format string used to build the summary text.
  - `TextSummaryDisplayed`: Indicates whether text summary is currently displayed.
  - `VerticalContentAlignment`: Vertical alignment for the content.
- **Methods**:
  - `ClearCommand`: Clears the selected items.
- **Events**:
  - `SelectionChanged`: Occurs when the selection of items changes.

### CheckBoxGroup
A panel that arranges checkboxes in a grid layout.
- **Properties**:
  - `CheckBoxButtonMargin`: Margin applied to checkboxes.
  - `Columns`: Number of columns in the layout.
  - `FirstColumn`: Index of the first column.
  - `ItemList`: Comma-delimited text for checkboxes.
  - `Orientation`: Whether the list fills vertically or horizontally.
  - `Rows`: Number of rows in the layout.
  - `SelectedItemValue`: Value of the selected item.
  - `SelectedValue`: Numeric value of the selected item.
- **Methods**:
  - `GetItemAtValue(int value)`: Gets the item text at the given numeric value index.
  - `LoadContent(List<string> list)`: Loads checkboxes from a list of strings.
  - `LoadContent(string[] list)`: Loads checkboxes from an array of strings.
- **Events**:
  - `SelectionChanged`: Raised when the selection changes.

### RadioCheckEnumGroup
A panel that arranges radio buttons or checkboxes based on an enum type.
- **Properties**:
  - `ButtonMargin`: Margin applied to buttons.
  - `Columns`: Number of columns in the layout.
  - `EnumType`: Enum type used to generate buttons.
  - `FirstColumn`: Index of the first column.
  - `Orientation`: Whether the list fills vertically or horizontally.
  - `Rows`: Number of rows in the layout.
  - `SelectedEnumValue`: Selected enum value.
- **Methods**:
  - `GetSelectedValueAsEnum<T>()`: Gets the selected value cast to the requested enum type.
- **Events**:
  - `SelectionChanged`: Raised when the selection changes.

### SideBySideListSelect
Control that presents two lists side-by-side with commands to move items between them.
- **Properties**:
  - `AllowCopiesInSelected`: Indicates whether copies are allowed when moving items to the selected list.
  - `HeaderFontStyle`: Font style for headers.
  - `HeaderFontWeight`: Font weight for headers.
  - `HeaderMargin`: Margin applied to headers.
  - `LeftHeaderText`: Header text for the left list.
  - `LeftListBoxForeground`: Foreground brush for the left list box.
  - `LeftListTemplate`: Template for the left list items.
  - `ListBackground`: Background brush for the lists.
  - `ListBoxBorderBrush`: Border brush for the list boxes.
  - `ListBoxBorderThickness`: Border thickness for the list boxes.
  - `ListSelectionMode`: Selection mode used for the lists.
  - `RightHeaderText`: Header text for the right list.
  - `RightListBoxForeground`: Foreground brush for the right list box.
  - `RightListTemplate`: Template for the right list items.
- **Methods**:
  - `GetSelectedItems<T>()`: Gets the selected items cast to the requested type.
  - `GetUnselectedItems<T>()`: Gets the unselected items cast to the requested type.
  - `LoadLists(IEnumerable leftItems, IEnumerable rightItems, string? descriptionProperty)`: Loads both lists from the provided enumerables.
- **Commands**:
  - `AddLeftToRightCommand`: Adds selected left items to the right list.
  - `AddAllLeftToRightCommand`: Adds all left items to the right list.
  - `AddRightToLeftCommand`: Adds selected right items back to the left list.
  - `AddAllRightToLeftCommand`: Moves all right items back to the left list.
- **Events**:
  - `SelectedCollectionChanged`: Raised when the selected collection changes.

### NumericTextBox
A TextBox control specialized for numeric input with formatting and bounds checking.
- **Properties**:
  - `CheckMaxMinValues`: Indicates whether maximum and minimum value checks are enforced.
  - `ClearToNull`: Indicates whether clearing sets the value to null.
  - `MaximumValue`: Maximum allowed value.
  - `MinimumValue`: Minimum allowed value.
  - `NumberDisplayFormat`: Display format string for the numeric value.
  - `NumberEntryType`: Numeric entry type used to validate text.
  - `NumericValue`: Numeric value represented by the control.
  - `NumericValueImmediateUpdate`: Indicates whether the numeric value is updated immediately on text change.
- **Methods**:
  - `ConvertTextToNumber()`: Converts the text to a numeric value.
  - `SetNumericValueFromText()`: Sets the numeric value based on the current text.
  - `SetTextFromNumericValue(bool isFocused)`: Sets the text based on the current numeric value.
- **Events**:
  - `OnClearEvent`: Raised when the text is cleared.

### GroupBox
A GroupBox control with a customizable header background.
- **Properties**:
  - `HeaderBackground`: Brush used for the header background.

### RadioGroup
A layout control that arranges radio buttons in a grid-like pattern with optional custom button.
- **Properties**:
  - `Columns`: Number of columns in the layout.
  - `CustomButtonEditValue`: Value entered in the custom button edit field.
  - `CustomButtonText`: Text displayed on the custom button.
  - `FirstColumn`: Index of the first column.
  - `HasCustomButton`: Indicates whether a custom radio button with an edit field is present.
  - `ItemList`: Comma-delimited text for radio buttons.
  - `Orientation`: Whether the list fills vertically or horizontally.
  - `RadioButtonMargin`: Margin applied to radio buttons.
  - `Rows`: Number of rows in the layout.
  - `SelectedItemValue`: String representation of the selected item.
  - `SelectedValue`: Numeric index of the selected value.
- **Methods**:
  - `GetItemAtValue(int value)`: Gets the item text at the given numeric value index.
  - `LoadContent(List<string> list)`: Loads radio buttons from a list of strings.
  - `LoadContent(string[] list)`: Loads radio buttons from an array of strings.
- **Events**:
  - `SelectionChanged`: Raised when the selection changes.

### TextBoxEx
An extended TextBox with additional features like clear button, suffix, and select-all behavior on focus.
- **Properties**:
  - `AllowClear`: Indicates whether the clear button is enabled.
  - `InnerLeftPadding`: Inner left padding to shift content.
  - `SelectAllOnGetFocus`: Indicates whether all text should be selected when the control receives focus.
  - `ShowPassword`: Indicates whether a reveal password button is shown when the input is password.
  - `Suffix`: Suffix text displayed alongside the input.
  - `SuffixPadding`: Padding applied to the suffix content.
- **Methods**:
  - `Clear()`: Clears the text.
- **Events**:
  - `OnClearEvent`: Raised when the text is cleared.

### TextBlockEx
An extended TextBlock with additional padding behavior based on font style.

### ComboBoxEx
An extended ComboBox with additional features such as clear button and customizable glyph.
- **Properties**:
  - `ClearButtonBackground`: Background brush for the clear button.
  - `DropGlyphPath`: Geometry for the drop glyph icon.
  - `DropGlyphSize`: Size of the drop glyph icon.
  - `GlyphType`: Glyph type used for the drop icon.
  - `PlaceholderFontStyle`: Placeholder font style.
  - `ShowClearButton`: Indicates whether the clear button is shown.
  - `TextSearchPropertyName`: Property name used for text search.
- **Methods**:
  - `ClearCommandExecute()`: Clears the selected value.

### EnumComboBox
A ComboBox for selecting enum values with descriptions.
- **Properties**:
  - `EnumType`: Enum type used for the items.
- **Methods**:
  - `MakeEnumDescArray()`: Creates an array of enum descriptions.

### ExpanderEx
An extended Expander with customizable header styles.
- **Properties**:
  - `HeaderBackground`: Background brush for the header.
  - `HeaderFontFamily`: Font family for the header.
  - `HeaderFontSize`: Font size for the header.
  - `HeaderFontStretch`: Font stretch for the header.
  - `HeaderFontStyle`: Font style for the header.
  - `HeaderFontWeight`: Font weight for the header.
  - `HeaderText`: Text displayed in the header.

### HiddenContainerControl
A container that can hide its child from layout and rendering while optionally drawing a background.
- **Properties**:
  - `Background`: Background brush used to fill the control.
  - `IsHidden`: Indicates whether the child content is hidden from layout and rendering.

### TransparentButton
A button with a transparent background and border.