# Maintainer: Jiří Kořenek
pkgname=fuzzy
pkgver=0.0.1
pkgrel=1
pkgdesc="Dynamic live fuzzy finder in C# using ripgrep"
arch=('x86_64')
url="https://github.com/wallach-game/fuzzy-csharp"
license=('MIT')
depends=('dotnet-runtime' 'ripgrep')
makedepends=('dotnet-sdk')
source=("git+https://github.com/wallach-game/fuzzy-csharp.git")
sha256sums=('SKIP')

build() {
    cd "$srcdir/$pkgname"
    dotnet publish -c Release -o "$srcdir/$pkgname/publish"
}

package() {
    install -d "$pkgdir/usr/lib/fuzzy"
    install -d "$pkgdir/usr/bin"

    # Copy published files (native binary + dependencies)
    cp -r "$srcdir/$pkgname/publish/"* "$pkgdir/usr/lib/fuzzy/"

    # Ensure native binary is executable
    chmod +x "$pkgdir/usr/lib/fuzzy/fuzzy"

    cp "./fuzzy" "$pkgdir/usr/bin/fuzzy"

    chmod +x "$pkgdir/usr/bin/fuzzy"
}
