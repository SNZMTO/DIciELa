"""Les images du README (Docs/Vitrine/), dans le style de l'interface du jeu.

Elles sont faites des images d'interface du jeu (Assets/UI/Images : le panneau de bois brun, le ruban de brique, la plaque
dorée ; aucune n'est tirée du pack), de ses polices (Lilita One et Nunito) et des couleurs de ses feuilles de style, et d'une
capture de l'album pour le portrait de la Chimère. Elles se refont après un changement de texte ou de chiffre.

Il faut Python 3 et Pillow (pip install pillow). Usage, depuis la racine du projet : python Tools/vitrine_images.py
"""

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

RACINE = Path(__file__).resolve().parent.parent
UI = RACINE / "Assets" / "UI"
SORTIE = RACINE / "Docs" / "Vitrine"

# Tout est dessiné deux fois plus grand qu'affiché, pour rester net sur un écran dense.
S = 2

# Les couleurs, telles que les feuilles de style du jeu les nomment (Assets/UI/*.uss).
LIN = (252, 238, 214)
SABLE = (239, 211, 164)
BEURRE = (248, 222, 149)
BOIS_NUIT = (90, 52, 23)
ENCRE = (74, 46, 59)
OMBRE_DU_TITRE = (28, 16, 8, 217)
OMBRE_DU_BOUTON = (60, 34, 14, 230)

# Les chiffres du projet : à tenir à jour (DECISIONS.md, la dernière passe de tests, le menu DICIELA, les .asmdef).
CHIFFRES = [("233", "décisions"), ("540", "tests"), ("35", "outils"), ("13", "assemblages")]

# Les rubans des sections du README : le nom du fichier, et ce qui y est écrit.
RUBANS = [
    ("ruban-le-jeu", "Le jeu"),
    ("ruban-comment-je-le-construis", "Comment je le construis"),
    ("ruban-architecture", "Une architecture qui se vérifie"),
    ("ruban-outils", "Un éditeur qui monte le projet"),
    ("ruban-hors-depot", "Ce qui n'est pas ici"),
]


def police(nom: str, taille: float) -> ImageFont.FreeTypeFont:
    return ImageFont.truetype(str(UI / "Polices" / nom), int(taille * S))


LILITA = "LilitaOne-Regular.ttf"
NUNITO_FORT = "Nunito-ExtraBold.ttf"
NUNITO = "Nunito-Bold.ttf"


def neuf_parts(image: Image.Image, largeur: int, hauteur: int, g: int, d: int, h: int, b: int) -> Image.Image:
    """Étire une image en neuf parts, comme l'interface du jeu : les coins tels quels, les bords et le milieu étirés."""
    w, hh = image.size
    sortie = Image.new("RGBA", (largeur, hauteur), (0, 0, 0, 0))
    xs = [(0, g, 0, g), (g, w - d, g, largeur - d), (w - d, w, largeur - d, largeur)]
    ys = [(0, h, 0, h), (h, hh - b, h, hauteur - b), (hh - b, hh, hauteur - b, hauteur)]
    for sx0, sx1, dx0, dx1 in xs:
        for sy0, sy1, dy0, dy1 in ys:
            if sx1 > sx0 and sy1 > sy0 and dx1 > dx0 and dy1 > dy0:
                sortie.paste(image.crop((sx0, sy0, sx1, sy1)).resize((dx1 - dx0, dy1 - dy0), Image.LANCZOS), (dx0, dy0))
    return sortie


def ui(nom: str) -> Image.Image:
    return Image.open(UI / "Images" / f"{nom}.png").convert("RGBA")


def ecrire(dessin: ImageDraw.ImageDraw, xy, texte: str, fonte, couleur, ombre=OMBRE_DU_TITRE, ancre="mm") -> None:
    """Un texte avec l'ombre portée dure des titres du jeu (2 px en bas à droite)."""
    x, y = xy
    dessin.text((x + 2 * S, y + 2 * S), texte, font=fonte, fill=ombre, anchor=ancre)
    dessin.text((x, y), texte, font=fonte, fill=couleur, anchor=ancre)


def ruban(texte: str, hauteur: float = 72) -> Image.Image:
    """Le ruban de brique du titre de l'écran de création, à la largeur de son texte."""
    fonte = police(LILITA, hauteur * 0.5)
    largeur_du_texte = fonte.getlength(texte)
    source = ui("Ruban")
    k = hauteur * S / source.height
    bord = int(64 * k)
    largeur = int(largeur_du_texte + 2 * bord + 40 * S)
    image = neuf_parts(source.resize((int(source.width * k), int(hauteur * S)), Image.LANCZOS), largeur, int(hauteur * S), bord, bord, 0, 0)
    ecrire(ImageDraw.Draw(image), (largeur / 2, hauteur * S / 2 - 2 * S), texte, fonte, LIN)
    return image


def plaque(texte: str, largeur: float, hauteur: float, taille: float) -> Image.Image:
    """La plaque dorée du bouton « C'est parti »."""
    image = neuf_parts(ui("Valider"), int(largeur * S), int(hauteur * S), 30, 30, 24, 24)
    ecrire(ImageDraw.Draw(image), (largeur * S / 2, hauteur * S / 2 - S), texte, police(LILITA, taille), LIN, OMBRE_DU_BOUTON)
    return image


def portrait(diametre: float) -> Image.Image:
    """La Chimère, prise dans l'écran de création de l'album, dans un médaillon de bois."""
    capture = Image.open(RACINE / "Docs" / "Album" / "2026-10-01_etape-18a_creation-bois-brun.jpg").convert("RGBA")
    d = int(diametre * S)
    visage = capture.crop((250, 40, 770, 560)).resize((d, d), Image.LANCZOS)
    masque = Image.new("L", (d, d), 0)
    ImageDraw.Draw(masque).ellipse((0, 0, d - 1, d - 1), fill=255)
    bord = int(10 * S)
    medaillon = Image.new("RGBA", (d + 2 * bord, d + 2 * bord), (0, 0, 0, 0))
    m = ImageDraw.Draw(medaillon)
    m.ellipse((0, 0, d + 2 * bord - 1, d + 2 * bord - 1), fill=BOIS_NUIT)
    m.ellipse((bord // 2, bord // 2, d + 2 * bord - bord // 2 - 1, d + 2 * bord - bord // 2 - 1), fill=BEURRE)
    medaillon.paste(visage, (bord, bord), masque)
    return medaillon


def banniere() -> Image.Image:
    largeur, hauteur = 1280, 360
    image = neuf_parts(ui("Panneau"), largeur * S, hauteur * S, 64, 64, 64, 64)
    medaillon = portrait(250)
    image.alpha_composite(medaillon, (int(70 * S), (hauteur * S - medaillon.height) // 2))
    milieu = 400 + (largeur - 400) / 2
    titre = ruban("DIciELa", 96)
    image.alpha_composite(titre, (int(milieu * S - titre.width / 2), int(46 * S)))
    dessin = ImageDraw.Draw(image)
    ecrire(dessin, (milieu * S, 186 * S), "Un jeu de gestion fantasy et cozy", police(NUNITO_FORT, 34), LIN)
    lignes = [
        "À terme : des chaînes de production tenues par de petits habitants,",
        "le troc à l'étal, et la Chimère qui met la patte à tout.",
    ]
    for rang, ligne in enumerate(lignes):
        ecrire(dessin, (milieu * S, (232 + rang * 30) * S), ligne, police(NUNITO, 21), SABLE, (28, 16, 8, 150))
    etiquette = plaque("Prototype en cours · Unity 6.6 · C#", 380, 50, 21)
    image.alpha_composite(etiquette, (int(milieu * S - etiquette.width / 2), int(284 * S)))
    return image


def chiffres() -> Image.Image:
    largeur, hauteur = 1280, 150
    image = neuf_parts(ui("Panneau"), largeur * S, hauteur * S, 64, 64, 64, 64)
    pas = (largeur - 80) / len(CHIFFRES)
    for rang, (nombre, mot) in enumerate(CHIFFRES):
        p = plaque(f"{nombre} {mot}", pas - 30, 76, 30)
        x = 40 + rang * pas + 15
        image.alpha_composite(p, (int(x * S), int((hauteur - 76) / 2 * S)))
    return image


def enregistrer(image: Image.Image, nom: str) -> None:
    chemin = SORTIE / f"{nom}.webp"
    image.save(chemin, "WEBP", quality=90, method=6)
    print(f"{chemin.relative_to(RACINE)}  {image.width // S} x {image.height // S}  {chemin.stat().st_size // 1024} Ko")


def main() -> None:
    SORTIE.mkdir(parents=True, exist_ok=True)
    enregistrer(banniere(), "banniere")
    enregistrer(chiffres(), "chiffres")
    for nom, texte in RUBANS:
        enregistrer(ruban(texte), nom)


if __name__ == "__main__":
    main()
