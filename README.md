# AnimationClipMaker
Unity用のEditor拡張コード。Spite(Multiple)からtsvデータに従ってAnimationClipを生成します。
生成部分は安藤圭吾氏のUnite2015のコードをそのまま利用させて頂いてます。(MIT LICENSE、ソース参照)

- 機能<br>
MultipleのSpriteから、tsvパラメータに従い、ごく単純なAnimationClipを生成します。<br>
　(Spriteを任意の順序で並べるだけ)<br>

- 導入方法<br>
Assets配下に「Editor」フォルダを作成し、AnimationClipMakerを配置して下さい。<br>
<br>

- 使用方法<br>
※元ネタとなるMultipleのSpriteを、予めResources配下に準備して下さい。<br>
(1) Unityメニューの「Window」から「ClipMaker」を選択<br>
(2) 「Spriteオブジェクト名」に、元ネタとなるSpriteのpathを入力(Resourcesをrootとする)<br>
(3) tsvに、下記ルールのデータをtab区切りで必要行数分入力<br>
<br>

| 出力Clip名(Asset/Animation/Hoge.anim) | 1フレーム目のSprite名 | 2フレーム目のSprite名 | 3フレーム目の～ |
| ------ | ------ | ------ | ------ |
| Animation/Hoge | Hoge01 | Hoge02 | Hoge03 |

C#もUnityも覚えたてにつき、<br>
色々覚えて数年後に綺麗にできるといいなぁと言うノリで中身ぐちゃぐちゃです。<br>
ご自由にいじって下さい。。そして参考にさせて頂きたく。
