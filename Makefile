all:
	mcs ExampleForm.cs -r:System.Windows.Forms.dll,System.Drawing.dll
	mono ExampleForm.exe
