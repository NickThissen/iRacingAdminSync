// from http://blogs.msdn.com/b/jaredpar/archive/2008/05/16/switching-on-types.aspx

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swordfish.NET.Charts {
  /// <summary>
  /// 
  /// </summary>
  /// <example>
  /// // On an event handler work out the control type
  /// TypeSwitch.Do(
  ///  sender,
  ///  TypeSwitch.Case<Button>(() => textBox1.Text = "Hit a Button"),
  ///  TypeSwitch.Case<CheckBox>(x => textBox1.Text = "Checkbox is " + x.Checked),
  ///  TypeSwitch.Default(() => textBox1.Text = "Not sure what is hovered over"));
  /// </example>
  static class TypeSwitch {
    public class CaseInfo {
      public bool IsDefault {
        get;
        set;
      }
      public Type Target {
        get;
        set;
      }
      public Action<object> Action {
        get;
        set;
      }
    }

    public static void Do(object source, params CaseInfo[] cases) {
      var type = source.GetType();
      foreach(var entry in cases) {
        if(entry.IsDefault || type == entry.Target) {
          entry.Action(source);
          break;
        }
      }
    }

    public static CaseInfo Case<T>(Action action) {
      return new CaseInfo() {
        Action = x => action(),
        Target = typeof(T)
      };
    }

    public static CaseInfo Case<T>(Action<T> action) {
      return new CaseInfo() {
        Action = (x) => action((T)x),
        Target = typeof(T)
      };
    }

    public static CaseInfo Default(Action action) {
      return new CaseInfo() {
        Action = x => action(),
        IsDefault = true
      };
    }
  }
}
