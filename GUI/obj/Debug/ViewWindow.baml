   M S B A M L   `   `   `  ����>  :GUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:GUIGUI  5   	       GUI.ViewWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   #localclr-namespace:GUI     5   	     � Title$  
ViewWindow��5   	   $	��450��6   $	��800��6)   .��5	      � $��10q�6    � RowDefinitions 5
   
   �� 5      $��* �6   6)   �� 5      $��50 �6   6*   .+�5   
   � $��0��6    5      �� 5      $��30 �6    6.   �� 5      $��* �6    6-   .+�5      �� -   5        $��	codeLabel��5   #   $��0��6    �� TextAlignment$ Center=�64   $��3q�6K   $��0q�6W   ���#ddd.��6}   	Code View6�   ,� 5      $��1��6'   $��1q�64   
���   � ,� IsToolBarVisible .  " ,� VerticalScrollBarVisibility$
 Auto=�6u   .��5      /� -   5         �Name$ codeView��5       /� 
FontFamily$ Consolas)�6/    /� FontSize$ 12(�6E    /� 	PageWidth$	 800��6S   	 /� 
Background	 �#fff.T�5      J� -   5        $
 main��5       .*�6,   .*�5      �� $��NoWrap=�6$   6<   5      5      5      5      �� 5   
   $��1��6   
 �� Orientation$
 
Horizontal=�6"   $��10q�6;   $��Center=�6G   .+�5      �� -   5        $��
exitButton��5      $��20��6'   $��50��63   .��6W   Exit6]   5      5      5       