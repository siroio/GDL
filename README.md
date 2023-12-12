## GDL ([gallery-dl](https://github.com/mikf/gallery-dl) wrapper)

## コマンドライン引数
- -list [file] : ダウンロードするURLを含むファイルを指定してください。(カンマ スペース 改行, 区切りに対応)

## 設定

#### cnf.ini ファイル内に設定を書き込んでください

- CustomArgs : gallery-dlに引数を渡せます
  - 詳しい内容は[ここ](https://github.com/mikf/gallery-dl?tab=readme-ov-file#usage)
- DownloadCount : 同時にダウンロード出来る数

## 使用方法
```shell
> ./GDL.exe -list URL.txt
```
```shell
> ./GDL.exe
```

<br>
終了するには q か quit を入力
