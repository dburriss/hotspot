namespace Clam

open System
open System.IO
open FSharp.Core

type Id = {
    name : string
    id : int
}

type Input = Array

type ArgSettings =
    /// Specifies that an arg must be used
    | Required
    /// Allows an arg to accept multiple values
    | MultipleValues
    /// Allows an arg to appear multiple times
    | MultipleOccurrences
    /// Allows an arg accept empty values such as `""`
    | AllowEmptyValues
    /// Hides an arg from the help message
    | Hidden
    /// Allows an argument to take a value (such as `--option value`)
    | TakesValue
    /// Enables a delimiter to break up arguments `--option val1,val2,val3` becomes three values
    /// (`val1`, `val2`, and `val3`) instead of the default one (`val1,val2,val3`)
    | UseValueDelimiter
    /// Tells an arg to display it's help on the line below the arg itself in the help message
    | NextLineHelp
    /// Says that arg *must* use a delimiter to separate values
    | RequireDelimiter
    /// Hides the possible values from the help message
    | HidePossibleValues
    /// Allows values that start with a hyphen
    | AllowHyphenValues
    /// Requires that an equals be used to provide a value to an option such as `--option=value`
    | RequireEquals
    /// Says that a positional arg will be the last positional, and requires `--` to be accessed.
    /// It can also be accessed early (i.e. before other positionals) by providing `--`
    | Last
    /// Hides the default value from the help message
    | HideDefaultValue
    /// Possible values become case insensitive
    | IgnoreCase
    /// Hides environment variable arguments from the help message
    | HideEnv
    /// Hides any values currently assigned to ENV variables in the help message (good for sensitive
    /// information)
    | HideEnvValues
    /// The argument should **not** be shown in short help text
    | HiddenShortHelp
    /// The argument should **not** be shown in long help text
    | HiddenLongHelp
    | RequiredUnlessAll
    
/// The abstract representation of a command line argument. Used to set all the options and
/// relationships that define a valid argument for the program.
type Arg = {
    id : Id
    name : string
    about : string option
    short : char option
    long : string option
    dispOrd : UInt32
    unifiedOrd : UInt32
    longAbout : string option
    valDelim : char option
    index : UInt32 option
    isGlobal : bool
}

type AppSettings =
    /// Specifies that any invalid UTF-8 code points should *not* be treated as an error.
    /// This is the default behavior of `clap`.
    ///
    /// **NOTE:** Using argument values with invalid UTF-8 code points requires using
    /// `ArgMatches.os_value_of`, `ArgMatches.os_values_of`, `ArgMatches.lossy_value_of`,
    /// or `ArgMatches.lossy_values_of` for those particular arguments which may contain invalid
    /// UTF-8 values
    | AllowInvalidUtf8

    /// Specifies that leading hyphens are allowed in argument *values*, such as negative numbers
    /// like `-10`. (which would otherwise be parsed as another flag or option)
    ///
    /// **NOTE:** Use this setting with caution as it silences certain circumstances which would
    /// otherwise be an error (such as accidentally forgetting to specify a value for leading
    /// option). It is preferred to set this on a per argument basis, via `Arg:.allow_hyphen_values`
    | AllowLeadingHyphen

    /// Specifies that all arguments override themselves. This is the equivolent to saying the `foo`
    /// arg using `Arg.overrides_with("foo")` for all defined arguments.
    | AllArgsOverrideSelf

    /// Allows negative numbers to pass as values. This is similar to
    /// `AllowLeadingHyphen` except that it only allows numbers, all
    /// other undefined leading hyphens will fail to parse.
    | AllowNegativeNumbers

    /// Allows one to implement two styles of CLIs where positionals can be used out of order.
    ///
    /// The first example is a CLI where the second to last positional argument is optional, but
    /// the final positional argument is required. Such as `$ prog [optional] <required>` where one
    /// of the two following usages is allowed:
    ///
    /// * `$ prog [optional] <required>`
    /// * `$ prog <required>`
    ///
    /// This would otherwise not be allowed. This is useful when `[optional]` has a default value.
    ///
    /// **Note:** when using this style of "missing positionals" the final positional *must* be
    /// [required] if `--` will not be used to skip to the final positional argument.
    ///
    /// **Note:** This style also only allows a single positional argument to be "skipped" without
    /// the use of `--`. To skip more than one, see the second example.
    ///
    /// The second example is when one wants to skip multiple optional positional arguments, and use
    /// of the `--` operator is OK (but not required if all arguments will be specified anyways).
    ///
    /// For example, imagine a CLI which has three positional arguments `[foo] [bar] [baz]...` where
    /// `baz` accepts multiple values (similar to man `ARGS...` style training arguments).
    ///
    /// With this setting the following invocations are posisble:
    ///
    /// * `$ prog foo bar baz1 baz2 baz3`
    /// * `$ prog foo -- baz1 baz2 baz3`
    /// * `$ prog -- baz1 baz2 baz3`
    | AllowMissingPositional

    /// Specifies that an unexpected positional argument,
    /// which would otherwise cause a [`ErrorKind::UnknownArgument`] error,
    /// should instead be treated as a [``] within the [`ArgMatches`] struct.
    ///
    /// **NOTE:** Use this setting with caution,
    /// as a truly unexpected argument (i.e. one that is *NOT* an external subcommand)
    /// will **not** cause an error and instead be treated as a potential subcommand.
    /// One should check for such cases manually and inform the user appropriately.
    | AllowExternalSubcommands

    /// Specifies that use of a valid [argument] negates [subcomands] being used after. By default
    /// `clap` allows arguments between subcommands such as
    /// `<cmd> [cmd_args] <cmd2> [cmd2_args] <cmd3> [cmd3_args]`. This setting disables that
    /// functionality and says that arguments can only follow the *final* subcommand. For instance
    /// using this setting makes only the following invocations possible:
    ///
    /// * `<cmd> <cmd2> <cmd3> [cmd3_args]`
    /// * `<cmd> <cmd2> [cmd2_args]`
    /// * `<cmd> [cmd_args]`
    | ArgsNegateSubcommands

    /// Specifies that the help text should be displayed (and then exit gracefully),
    /// if no arguments are present at runtime (i.e. an empty run such as, `$ myprog`.
    | ArgRequiredElseHelp

    /// Instructs the parser to stop when encountering a subcommand instead of greedily consuming
    /// args.
    ///
    /// By default, if an option taking multiple values is followed by a subcommand, the
    /// subcommand will be parsed as another value.
    ///
    /// ```text
    /// app --foo val1 val2 subcommand
    ///           --------- ----------
    ///             values   another value
    /// ```
    ///
    /// This setting instructs the parser to stop when encountering a subcommand instead of
    /// greedily consuming arguments.
    ///
    /// ```text
    /// app --foo val1 val2 subcommand
    ///           --------- ----------
    ///             values   subcommand
    /// ```
    ///
    /// **Note:** Make sure you apply it as `global_setting` if you want it to be propagated to
    /// sub-sub commands!
    | SubcommandPrecedenceOverArg

    /// Uses colorized help messages.
    | ColoredHelp

    /// Enables colored output only when the output is going to a terminal or TTY.
    ///
    /// **NOTE:** This is the default behavior of `clam`.
    | ColorAuto

    /// Enables colored output regardless of whether or not the output is going to a terminal/TTY.
    | ColorAlways

    /// Disables colored output no matter if the output is going to a terminal/TTY, or not.
    | ColorNever

    /// Disables the automatic collapsing of positional args into `[ARGS]` inside the usage string
    | DontCollapseArgsInUsage

    /// Disables the automatic delimiting of values when `--` or `AppSettings.TrailingVarArg`
    /// was used.
    ///
    /// **NOTE:** The same thing can be done manually by setting the final positional argument to
    /// [`Arg::use_delimiter(false)`]. Using this setting is safer, because it's easier to locate
    /// when making changes.
    | DontDelimitTrailingValues

    /// Disables `-h` and `--help` [`App`] without affecting any of the [`SubCommand`]s
    /// (Defaults to `false`; application *does* have help flags)
    | DisableHelpFlag

    /// Disables the `help` subcommand
    | DisableHelpSubcommand

    /// Disables `-V` and `--version` for this [`App`] without affecting any of the [``]s
    /// (Defaults to `false`; application *does* have a version flag)
    | DisableVersionFlag

    /// Disables `-V` and `--version` for all [`subcommand`]s of this [`App`]
    /// (Defaults to `false`; subcommands *do* have version flags.)
    | DisableVersionForSubcommands

    /// Displays the arguments and [``]s in the help message in the order that they were
    /// declared in, and not alphabetically which is the default.
    | DeriveDisplayOrder

    /// Specifies to use the version of the current command for all child [``]s.
    /// (Defaults to `false`; subcommands have independent version strings from their parents.)
    | GlobalVersion

    /// Specifies that this [``] should be hidden from help messages
    | Hidden

    /// Tells `clam` *not* to print possible values when displaying help information.
    /// This can be useful if there are many values, or they are explained elsewhere.
    | HidePossibleValuesInHelp

    /// Tells `clam` to panic if help strings are omitted
    | HelpRequired

    /// Tries to match unknown args to partial [`subcommands`] or their [aliases]. For example, to
    /// match a subcommand named `test`, one could use `t`, `te`, `tes`, and `test`.
    ///
    /// **NOTE:** The match *must not* be ambiguous at all in order to succeed. i.e. to match `te`
    /// to `test` there could not also be a subcommand or alias `temp` because both start with `te`
    ///
    /// **CAUTION:** This setting can interfere with [positional/free arguments], take care when
    /// designing CLIs which allow inferred subcommands and have potential positional/free
    /// arguments whose values could start with the same characters as subcommands. If this is the
    /// case, it's recommended to use settings such as `AppSettings.ArgsNegateSubcommands` in
    /// conjunction with this setting.
    | InferSubcommands

    /// Specifies that the parser should not assume the first argument passed is the binary name.
    /// This is normally the case when using a "daemon" style mode, or an interactive CLI where one
    /// one would not normally type the binary or program name for each command.
    | NoBinaryName

    /// Places the help string for all arguments on the line after the argument.
    | NextLineHelp

    /// Allows [``]s to override all requirements of the parent command.
    /// For example, if you had a subcommand or top level application with a required argument
    /// that is only required as long as there is no subcommand present,
    /// using this setting would allow you to set those arguments to `Arg.required(true)`
    /// and yet receive no error so long as the user uses a valid subcommand instead.
    ///
    /// **NOTE:** This defaults to false (using subcommand does *not* negate requirements)
    | SubcommandsNegateReqs

    /// Specifies that the help text should be displayed (before exiting gracefully) if no
    /// [``]s are present at runtime (i.e. an empty run such as `$ myprog`).
    ///
    /// **NOTE:** This should *not* be used with `AppSettings.SubcommandRequired` as they do
    /// nearly same thing; this prints the help text, and the other prints an error.
    ///
    /// **NOTE:** If the user specifies arguments at runtime, but no subcommand the help text will
    /// still be displayed and exit. If this is *not* the desired result, consider using
    /// `AppSettings.ArgRequiredElseHelp` instead.
    | SubcommandRequiredElseHelp

    /// Specifies that any invalid UTF-8 code points should be treated as an error and fail
    /// with a [`ErrorKind::InvalidUtf8`] error.
    ///
    /// **NOTE:** This rule only applies to argument values; Things such as flags, options, and
    /// [``]s themselves only allow valid UTF-8 code points.
    | StrictUtf8

    /// Allows specifying that if no [``] is present at runtime,
    /// error and exit gracefully.
    ///
    /// **NOTE:** This defaults to `false` (subcommands do *not* need to be present)
    | SubcommandRequired

    /// Specifies that the final positional argument is a "VarArg" and that `clap` should not
    /// attempt to parse any further args.
    ///
    /// The values of the trailing positional argument will contain all args from itself on.
    ///
    /// **NOTE:** The final positional argument **must** have [`Arg::multiple(true)`] or the usage
    /// string equivalent.
    | TrailingVarArg

    /// Groups flags and options together, presenting a more unified help message
    /// (a la `getopts` or `docopt` style).
    ///
    /// The default is that the auto-generated help message will group flags, and options
    /// separately.
    ///
    /// **NOTE:** This setting is cosmetic only and does not affect any functionality.
    | UnifiedHelpMessage

    /// Will display a message "Press \[ENTER\]/\[RETURN\] to continue..." and wait for user before
    /// exiting
    ///
    /// This is most useful when writing an application which is run from a GUI shortcut, or on
    /// Windows where a user tries to open the binary by double-clicking instead of using the
    /// command line.
    ///
    /// **NOTE:** This setting is **not** recursive with [``]s, meaning if you wish this
    /// behavior for all subcommands, you must set this on each command (needing this is extremely
    /// rare)
    | WaitOnError

    /// @TODO-v1: @docs write them...maybe rename
    | NoAutoHelp

    /// @TODO-v1: @docs write them...maybe rename
    | NoAutoVersion

    | LowIndexMultiplePositional

    | TrailingValues

    | ValidNegNumFound

    | Built

    | ValidArgFound

    | ContainsLast


/// Represents a command line interface which is made up of all possible
/// command line arguments and subcommands. Interface arguments and settings are
/// configured using the "builder pattern." Once all configuration is complete,
/// the `App.getMatches` family of methods starts the runtime-parsing
/// process. These methods then return information about the user supplied
/// arguments (or lack thereof).
type App = {
    id : Id
    name : string
    longFlag : string option
    shortFlag : char option
    author : string option
    about : string option
    longAbout : string option
    helpAbout : string option
    helpStr : string option
    args : Map<int, Arg>
    commands : App ResizeArray
    version : string option
    longVersion : string option
    appSettings : Set<AppSettings>
}

module String =
    let hash (n : string) = n.GetHashCode()
    let trim (s :string) = s.Trim()
    let replace (oldValue : string) newValue (s : string) = s.Replace(oldValue, newValue)

module ResizeArray =
    let empty<'a> = ResizeArray<'a>()
    let map f (arr : ResizeArray<'a>) = arr |> Seq.map f |> ResizeArray  
    let iter<'a> (f : 'a -> unit) (arr : ResizeArray<'a>) = arr |> Seq.iter f
    let filter<'a> predicate (arr : ResizeArray<'a>) = arr |> Seq.filter predicate
    let exists predicate (arr : ResizeArray<'a>) = arr |> Seq.exists predicate

module Id =
    let from (n : string) = {
          name = n
          id = String.hash n
        }

module Arg =
    let withName (name) : Arg =
        {
            id = Id.from name
            name = name
            about = None
            short = None
            long = None
            dispOrd = 999u
            unifiedOrd = 999u
            longAbout = None
            valDelim = None
            index = None
            isGlobal = false
        }
    
    let hasSwitch (arg : Arg) = Option.isSome arg.short || Option.isSome arg.long

module ArgMatcher =
    let contains (id : Id) =
        ()

module App =
    open Spectre.Console
    let console writer =
        let settings = AnsiConsoleSettings()
        settings.Ansi <- AnsiSupport.Detect
        settings.ColorSystem <- ColorSystemSupport.Detect
        settings.Out <- writer
        AnsiConsole.Create(settings)
    
    let create n = {
        id = Id.from n
        name = n
        longFlag = None
        shortFlag = None
        author = None
        about = None
        longAbout = None
        helpAbout = None
        helpStr = None
        args = Map.empty
        commands = ResizeArray.empty
        version = None
        longVersion = None
        appSettings = Set.empty
    }
    
    /// Sets a string of author(s) that will be displayed to the user when they
    /// request the help message.
    let author name (app : App) = { app with author = Some name }
    
    /// Sets a string describing what the program does. This will be displayed
    /// when the user requests the short format help message (`-h`).
    ///
    /// `clam` can display two different help messages, a [long format] and a
    /// [short format] depending on whether the user used `-h` (short) or
    /// `--help` (long). This method sets the message during the short format
    /// (`-h`) message. However, if no long format message is configured, this
    /// message will be displayed for *both* the long format, or short format
    /// help message.
    let about txt (app : App) = { app with about = Some txt }
    
     /// Allows the subcommand to be used as if it were an `Arg.short`.
    ///
    /// Sets the short version of the subcommand flag without the preceding `-`.
    let short c (app : App) = { app with shortFlag = Some c }
    
    /// Allows the subcommand to be used as if it were an `Arg.long`.
    ///
    /// Sets the long version of the subcommand flag without the preceding `--`.
    ///
    /// **NOTE:** Any leading `-` characters will be stripped.
    let long s (app : App) =
        let s = s |> (String.trim >> String.replace "-" "")
        { app with longFlag = Some s }
    
    /// Sets the help text for the auto-generated help argument and subcommand.
    ///
    /// By default clam sets this to "Prints help information" for the help
    /// argument and "Prints this message or the help of the given subcommand(s)"
    /// for the help subcommand but if you're using a different convention
    /// for your help messages and would prefer a different phrasing you can
    /// override it.
    ///
    /// **NOTE:** This setting propagates to subcommands.
    let helpAbout txt (app : App) = { app with helpAbout = Some txt }
    
    /// Overrides the auto-generated help text
    let overrideHelp txt (app : App) = { app with helpStr = Some txt }
    
    /// Get the version of the app/command
    let GetVersion (app : App) = app.version |> Option.defaultValue ""
    
    /// Get the help message specified via `App.about`.
    let GetAbout (app : App) = app.about |> Option.defaultValue ""
    
    let private deriveDisplayOrder (app : App) =
        // TODO: 08/11/2020 dburriss@xebia.com | Derive display order
        app
    
    let private hasHelp (app : App) =
        app.commands
        |> ResizeArray.exists (fun a -> a.name = "help")
        
    let private isHelp (app : App) = app.name = "help"
        
    let private hasVersion (app : App) =
        app.commands
        |> ResizeArray.exists (fun a -> a.name = "version")
        
    let private isVersion (app : App) = app.name = "version"
    let private isHelpOrVersion (app : App) = isHelp app || isVersion app 
    let private sPrintHelp (app : App) =
        sprintf
            "
%s %s %s
%s %s

Usage:

Options:

Commands:
            "
            app.name (app |> GetVersion) (Environment.NewLine)
            (app |> GetAbout) (Environment.NewLine)
            
        
    let rec private createHelpAndVersion (app : App) =
        let setDefaults (app : App) =
            app |> fun a ->
                let mutable updatedVersion =
                    { a with version = if Option.isNone a.version then Some "" else a.version }
                if a |> hasHelp |> not && app |> isHelpOrVersion |> not then
                    let help = create "help" |> about "Prints help information"
                    updatedVersion.commands.Add(help)
                if a |> hasVersion |> not && app |> isHelpOrVersion |> not then
                    let version = create "version" |> about "Display version info"
                    updatedVersion.commands.Add(version)
                updatedVersion <- { updatedVersion with appSettings = updatedVersion.appSettings.Add(AppSettings.Built) }
                updatedVersion
                
        let app = setDefaults app
        for i = 0 to (app.commands.Count - 1) do
            let cur = app.commands.[i]
            app.commands.[i] <- createHelpAndVersion cur
        app
        
    let _build (app : App) =
        app |> deriveDisplayOrder |> createHelpAndVersion
        // TODO: 08/11/2020 dburriss@xebia.com | Build indexes
    
    /// Writes the full help message to the user to a [`io::Write`] object in the same method as if
    /// the user ran `-h`.
    ///
    /// **NOTE:** clam has the ability to distinguish between "short" and "long" help messages
    /// depending on if the user ran [`-h` (short)] or [`--help` (long)].
    let writeHelp (writer : TextWriter) (app :App) =
        app |> _build
        |> fun a -> a.helpStr
                    |> Option.defaultWith (fun () -> sPrintHelp a)
                    |> fun s -> writer.Write(s) |> ignore
        writer
    
    /// Prints the full help message to stdout using the same
    /// method as if someone ran `-h` to request the help message.
    ///
    /// **NOTE:** clam has the ability to distinguish between "short" and "long" help messages
    /// depending on if the user ran [`-h` (short)] or [`--help` (long)].
    let printHelp (app : App) =
        let writer = new StringWriter()
        app
        |> _build
        |> writeHelp writer
        |> fun w -> w.ToString()
        |> fun h -> (console writer).WriteLine(h, Style.Plain)
        
    let arg (a : Arg) (app : App) =
        let args = app.args.Add(a.id.id, a)
        { app with args = args }
    
    let private doParse (input : Input) (app : App) =
        ()
    let getMatchesFrom (args: string array) (app : App) =
        let app = app |> _build
        
        app
        