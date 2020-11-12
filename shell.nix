{pkgs ? null} @ args:
let
  pinnedNixpkgsSrc = builtins.fetchTarball {
    url = "https://github.com/NixOS/nixpkgs/archive/f1f038331f538550072acd8845f2138de23cb401.tar.gz";
    # Get this info from the output of: `nix-prefetch-url --unpack $url` where `url` is the above.
    sha256 = "1y5gnbikhyk7bfxqn11fk7y49jad9nlmaq1mr4qzj6fnmrh807js";
  };

  pinnedNixpkgs = import pinnedNixpkgsSrc { config = {}; };

  pkgs = if args ? "pkgs" then args.pkgs else pinnedNixpkgs;
in

with pkgs;

let
  mono = mono6;
  dotnetCoreCombined = with dotnetCorePackages; combinePackages [
    sdk_3_1
  ];

in

mkShell {
  name = "dotnet-env";
  buildInputs = [
    dotnetCoreCombined
    mono
  ];

  # Allow us to use dotnet tools such as dotnet ef.
  shellHook = ''
    export "DOTNET_ROOT=${dotnetCoreCombined}"
    export "PATH=$HOME/.dotnet/tools:$PATH"
  '';
}
