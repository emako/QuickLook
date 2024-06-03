// Copyright © 2024 ema
//
// This file is part of QuickLook program.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace QuickLook.Plugin.BinaryViewer;

public partial class BinaryViewerPanel : UserControl
{
    private BinaryType binaryType = BinaryType.HEX;

    public BinaryType BinaryType
    {
        get => binaryType;
        set
        {
            if (value == BinaryType.NONE)
            {
                return;
            }

            if (binaryType != value)
            {
                binaryType = value;
                OnPropertyChanged(nameof(BinaryType));
            }
        }
    }

    public BinaryViewerPanel()
    {
        DataContext = this;
        InitializeComponent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void LoadTextAsync(string text)
    {
        TextEditor.LoadTextAsync(text);
    }
}

public enum BinaryType
{
    NONE = -1,
    HEX,
    BYTE,
    CHAR,
}

public sealed class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (parameter == null)
            return false;

        string parameterString = parameter.ToString();
        if (Enum.IsDefined(value.GetType(), value) == false)
            return false;

        object parameterValue = Enum.Parse(value.GetType(), parameterString);

        return parameterValue.Equals(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (parameter == null)
            return null!;

        string parameterString = (bool)value ? parameter.ToString() : (-1).ToString();
        return Enum.Parse(targetType, parameterString);
    }
}
