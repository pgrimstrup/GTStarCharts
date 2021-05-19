using PanasonicNZ.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PanasonicNZ.WPF
{
    /// <summary>
    /// Interaction logic for CheckedComboBox.xaml
    /// </summary>
    public partial class CheckedComboBox : UserControl
    {
        Type _keyType = typeof(int);

        public CheckedComboBox()
        {
            InitializeComponent();
            Height = PART_Combo.Height;
        }

        public Type KeyType
        {
            get { return GetValue(KeyTypeProperty) as Type; }
            set { SetValue(KeyTypeProperty, value); }
        }

        public System.Collections.IEnumerable ItemsSource
        {
            get
            {
                return PART_Combo.ItemsSource;
            }
            set
            {
                if (PART_Combo.ItemsSource != value)
                {
                    if (PART_Combo.ItemsSource is IBindingList oldbl)
                        oldbl.ListChanged -= ItemsSource_Changed;

                    PART_Combo.ItemsSource = value;

                    if (value is IBindingList newbl)
                        newbl.ListChanged += ItemsSource_Changed;
                }
            }
        }

        private void ItemsSource_Changed(object sender, ListChangedEventArgs e)
        {
            UpdateSelectedValues();
        }

        private void UpdateSelectedValues()
        {
            if (ItemsSource == null)
            {
                SelectedValuesText = "";
                return;
            }

            var items = ItemsSource.Cast<object>().Select(i => new
            {
                Id = i.GetValue("Id"),
                ParentId = i.GetValue("ParentId"),
                IsChecked = i.GetValue<bool>("IsChecked"),
                Name = i.GetValue<string>("Name"),
                Item = i
            }).ToDictionary(i => i.Id);

            string values = "";
            object selected = null;
            foreach (var item in items.Values.ToArray())
            {
                if (item.IsChecked)
                {
                    if (IsSelectAll(item.Id))
                    {
                        values = item.Name;
                        selected = item.Item;
                        break;
                    }
                    else if (item.ParentId != null && items.ContainsKey(item.ParentId))
                    {
                        // If the parent is checked, then don't add this item in
                        var parent = items[item.ParentId];
                        if (!parent.IsChecked)
                            values += item.Name + ", ";
                    }
                    else
                        values += item.Name + ", ";

                    if (selected == null)
                        selected = item.Item;
                }
            }

            SelectedValuesText = values.Trim().TrimEnd(',');
            PART_Combo.SelectedItem = selected;
        }

        private bool IsSelectAll(object id)
        {
            if (SelectAllValue != null && SelectAllValue.Equals(id))
                return true;
            return false;
        }

        public object SelectAllValue
        {
            get { return (string)GetValue(SelectAllValueProperty); }
            set { SetValue(SelectAllValueProperty, value); }
        }

        public string SelectedValuesText
        {
            get { return (string)GetValue(SelectedValuesTextProperty); }
            set { SetValue(SelectedValuesTextProperty, value); }
        }

        public object SelectedValue
        {
            get { return PART_Combo.SelectedValue; }
            set { PART_Combo.SelectedValue = value; }
        }

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(nameof(SelectedValue), typeof(object), typeof(CheckedComboBox), new PropertyMetadata(OnSelectedValueChanged));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(System.Collections.IEnumerable), typeof(CheckedComboBox), new PropertyMetadata(OnItemsSourceChanged));
        public static readonly DependencyProperty SelectAllValueProperty = DependencyProperty.Register(nameof(SelectAllValue), typeof(object), typeof(CheckedComboBox), new PropertyMetadata(OnSelectAllValueChanged));
        public static readonly DependencyProperty SelectedValuesTextProperty = DependencyProperty.Register(nameof(SelectedValuesText), typeof(string), typeof(CheckedComboBox), new PropertyMetadata());
        public static readonly DependencyProperty KeyTypeProperty = DependencyProperty.Register(nameof(KeyType), typeof(Type), typeof(CheckedComboBox), new PropertyMetadata(typeof(int), new PropertyChangedCallback(OnKeyTypeChanged)));


        static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckedComboBox cbo)
            {
                cbo.SelectedValue = e.NewValue;
                cbo.UpdateSelectedValues();
            }
        }

        static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckedComboBox cbo)
            {
                cbo.ItemsSource = (System.Collections.IEnumerable)e.NewValue;
                cbo.UpdateSelectedValues();
            }
        }

        static void OnSelectAllValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckedComboBox cbo)
                cbo.UpdateSelectedValues();
        }

        static void OnKeyTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CheckedComboBox cbo = d as CheckedComboBox;
            cbo?.OnKeyTypeChanged(e);
        }

        private void OnKeyTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            _keyType = e.NewValue as Type;
        }


        private void Combo_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb)
                if (cb.DataContext != null)
                    UncheckAllItemsIfNeeded(cb.DataContext.GetValue("Id"));

            OnPropertyChanged(new DependencyPropertyChangedEventArgs(SelectedValuesTextProperty, "", SelectedValuesText));
            UpdateSelectedValues();
        }

        private void Combo_Unchecked(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged(new DependencyPropertyChangedEventArgs(SelectedValuesTextProperty, "", SelectedValuesText));
            UpdateSelectedValues();
        }

        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string oldvalue = SelectedValuesText;
            if (sender is ComboBox cbo)
            {
                if (cbo.SelectedItem != null)
                {
                    cbo.SelectedItem.SetValue("IsChecked", true);
                    UncheckAllItemsIfNeeded(cbo.SelectedItem.GetValue("Id"));
                }
            }
            OnPropertyChanged(new DependencyPropertyChangedEventArgs(SelectedValuesTextProperty, oldvalue, SelectedValuesText));
            UpdateSelectedValues();
        }

        private void UncheckAllItemsIfNeeded(object checkedId)
        {
            if (IsSelectAll(checkedId))
            {
                // Switch off everything else
                if (ItemsSource != null)
                {
                    foreach (var item in ItemsSource)
                    {
                        var id = item.GetValue("Id");
                        if (!id.Equals(checkedId))
                            item.SetValue("IsChecked", false);
                    }
                }
            }
            else
            {
                // Switch off the Select All item
                if (ItemsSource != null)
                {
                    foreach (var item in ItemsSource)
                    {
                        var id = item.GetValue("Id");
                        if (IsSelectAll(id))
                            item.SetValue("IsChecked", false);
                    }
                }
            }
        }
    }

    public class CheckedComboBoxTemplateSelector : DataTemplateSelector
    {
        // Can set both templates from XAML
        public DataTemplate FaceTemplate { get; set; }
        public DataTemplate ItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            bool selected = false;

            // container is the ContentPresenter
            FrameworkElement fe = container as FrameworkElement;
            if (fe != null)
            {
                DependencyObject parent = fe.TemplatedParent;
                if (parent != null)
                {
                    ComboBox cbo = parent as ComboBox;
                    if (cbo != null)
                        selected = true;
                }
            }

            if (selected)
                return FaceTemplate;
            else
                return ItemTemplate;
        }
    }


}
