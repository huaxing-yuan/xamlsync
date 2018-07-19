# xamlsync
Xaml Sync is a tool used in Hummingbird project to synchroize missing XAML entry for different language resource files.
it is useful when you are working on multilanguage project and using XAML Resource Dictionary as solution.

## Automatic translation
The tool features also a cloud based translation service (with Azure) to translate automatically missing tag values to the target language. The missing value will be translated automatically to French from English.

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
<s:String x:Key="Application_Description">Une solution de test intégrée</s:String> 
```
Then it is easy to check the translation result manually by a humain.


## limitation
The tool works only with String resource dictionaries.
The source resource must be English.
Machine translation has their limit, the translation result must be checked by a humain ;)
