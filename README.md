# xamlsync
Xaml Sync is a tool used in Hummingbird project to synchroize missing XAML entry for different language resource files.

For example, considering follow XAML code fragments in Resources.en.xaml
```xml
<s:String x:Key="Application_Name">Hummingbird</s:String>
<!-- Description -->
<s:String x:Key="Application_Description">An integrated test solution</s:String>
```

and the XAML code fragement in Resources.fr.xaml
```xml
<s:String x:Key="Application_Name">Hummingbird</s:String>
```
After running:
> `xamlsync Resources.en.xaml`

the tool will add following entries to Resources.fr.xaml and other XAML files in the same folder.
```xml
<!-- Description -->
<s:String x:Key="Application_Description">An integrated test solution</s:String> 
```
Then it is easy to translate the value of each string.

## limitation
The tools works only with String resource dictionaries for instance.
