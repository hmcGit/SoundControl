# SoundControl
Unity SE Control Script(C#)


効果音（SE)管理クラス。オブジェクトにアタッチすることなくどこでも使用できる

    ＜使い方＞
    
    ◆再生：SoundControl.Instance.playSE(SE登録名)
    
    　SE登録名（キー）はstring。SE登録名（キー）はこのクラスの初期化で指定しておく
    　
    　
　◆初期化
　
      private SoundControl()にて
      
      SEを予め登録しておく。 登録には、呼び出しに指定するキーとファイル名を指定する
      
      
      ・「SE登録名（キー）」
      
      ・「SEのファイル名（Resourcesフォルダからのパスを記述」
      
      
