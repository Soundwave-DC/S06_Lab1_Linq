using LinqEtSeedEF.Data;
using LinqEtSeedEF.Models;
using LinqEtSeedEF.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinqEtSeedEF.Controllers
{
    public class HomeController : Controller
    {
        private readonly LinqEtSeedEFContext _context;

        public HomeController(LinqEtSeedEFContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Data()
        {
            DataViewModel dataViewModel = new DataViewModel();

            dataViewModel.Clients = await _context.Client.ToListAsync();
            dataViewModel.Commandes = await _context.Commande.ToListAsync();
            dataViewModel.CommandePlats = await _context.CommandePlat.OrderBy(cp => cp.CommandeId).ToListAsync();
            dataViewModel.Plats = await _context.Plat.ToListAsync();
            dataViewModel.Restaurants = await _context.Restaurant.ToListAsync();

            return View(dataViewModel);
        }

        public async Task<IActionResult> Questions()
        {
            QuestionsViewModel questionViewModel = new QuestionsViewModel();

            // ATTENTION: N'enlevez pas ces lignes de code qui semblent peut-être inutiles.
            // Nous allons parler de loading au prochain cours et nous allons voir une comment gérer le loading efficacement.
            // D'ici là, comprenez simplement que ces lignes load TOUTES les données des tables et les gardent en mémoire pour la durée de la requête.
            // Normalement, on ne veut PAS travailler de cette manière!
            //Début du code qu'il faut garder
            await _context.Client.ToListAsync();
            await _context.Commande.ToListAsync();
            await _context.CommandePlat.ToListAsync();
            await _context.Plat.ToListAsync();
            await _context.Restaurant.ToListAsync();
            //Fin du code qu'il faut garder

            questionViewModel.PrixPlatLePlusCher = PrixPlatLePlusCher();
            questionViewModel.ValeurTotalDesPlats = ValeurTotalDesPlats();
            questionViewModel.ValeurTotalDesCommandes = ValeurTotalDesCommandes("Patrick Gagné");
            questionViewModel.PrixCommandeLaPlusCher = PrixCommandeLaPlusCher();

            questionViewModel.VegetarienResto1 = Vegetarien("La graine du père George");
            questionViewModel.VegetarienResto2 = Vegetarien("Le Bistro");
            questionViewModel.VegetarienResto3 = Vegetarien("La Belle Province");

            questionViewModel.PlatsVege = PlatsVegeOrdeCroissantDePrix();
            questionViewModel.PlatsLesPlusChers = PlatsLesPlusChersOrdeDecroissantDePrix(3);

            return View(questionViewModel);
        }

        private DecimalViewModel PrixPlatLePlusCher()
        {
            // TODO: Écrire la logique pour trouver le prix du plat le plus cher avec une boucle
            var liste = _context.Plat.ToList();
            decimal prix = 0;
            foreach (Plat p in liste)
            {
                if (p.Prix > prix)
                    prix = p.Prix;
            }
            // TODO: Écrire la logique pour trouver le prix du plat le plus cher avec Linq
            // Utilisez Max
            decimal prixLinq = 0;
            prixLinq = liste.Max(liste => liste.Prix);

            return new DecimalViewModel("Quel est le prix du plat le plus cher?", prix, prixLinq);
        }

        private DecimalViewModel ValeurTotalDesPlats()
        {
            // TODO: Calculer la valeur totale des plats avec boucle et Linq
            // Utilisez Sum avec Linq
            var liste = _context.Plat.ToList();
            decimal valeur = 0;
            decimal valeurLinq = 0;
            foreach (Plat p in liste)
            {
                valeur += p.Prix;
            }

            valeurLinq = liste.Sum(liste => liste.Prix);
            return new DecimalViewModel("Quelle est la valeur totale des plats?", valeurLinq, valeur);
        }

        private DecimalViewModel ValeurTotalDesCommandes(string nomClient)
        {
            // TODO: Calculer la valeur totale des commandes du client [nomClient] avec boucle et Linq
            // Linq: Utilisez Where et 2 fois Sum
            var listeLinq = _context.Commande.ToList();
            var listePlats = _context.Plat.ToList();
            decimal valeurTotale = 0;
            foreach (Commande p in _context.Commande.ToList())
            {

                decimal somme = 0;

                if (p.Client.Nom == nomClient)
                {

                    foreach (CommandePlat commandeP in p.CommandesPlats)
                    {
                        somme += commandeP.Plat.Prix * commandeP.Quantite;
                    }
                }

                valeurTotale += somme;

            }
            // Attention: c'est plus facile si vous faites un ToList() et faites le linq sur la liste et non pas le DbSet
            // on en parlera au prochain cours
            // Faites votre requête Linq sur listeLinq
            var commandeCourante = listeLinq.ToList().Where(c => c.Client.Nom == nomClient);

            decimal totalPlats = commandeCourante.Sum(commande => commande.CommandesPlats.Sum(commandePlat => commandePlat.Quantite * commandePlat.Plat.Prix));



            return new DecimalViewModel("Quelle est la valeur totale des commandes de " + nomClient + "?", valeurTotale, totalPlats);
        }

        private DecimalViewModel PrixCommandeLaPlusCher()
        {
            // TODO: Trouver le côut total de la commande la plus chère
            decimal totalFinal = 0;
            foreach (Commande c in _context.Commande.ToList())
            {
                decimal totalTest = 0;
                foreach (CommandePlat p in c.CommandesPlats)
                {
                    totalTest += p.Plat.Prix * p.Quantite;
                }
                if (totalFinal < totalTest)
                {
                    totalFinal = totalTest;
                }
            }
            // Linq: Utilisez Sum et Max
            var listeLinq = _context.Commande.ToList();
            // Attention: c'est plus facile si vous faites un ToList() et faites le linq sur la liste et non pas le DbSet
            // on en parlera au prochain cours
            // Faites votre requête Linq sur listeLinq
            decimal totalFinalLinq = 0;
            totalFinalLinq = listeLinq.Max(commande => commande.CommandesPlats.Sum(CommandePlats => CommandePlats.Plat.Prix * CommandePlats.Quantite));

            return new DecimalViewModel("Quel est le prix de la commande la plus chère?", totalFinal, totalFinalLinq);
        }

        private VegetarienViewModel Vegetarien(string nomDuResto)
        {
            // TODO: Est-ce que le restaurant avec le nom [nomDuRest] a au moins un plat végé?
            bool? optionVege = null;
            var listeResto = _context.Restaurant.ToList();
            foreach (Restaurant resto in listeResto)
            {
                if (resto.Nom.ToString() == nomDuResto)
                {
                    optionVege = false;
                    foreach (Plat plat in resto.Plats)
                    {
                        if (plat.Vegetarien == true)
                        { 
                            optionVege = true;
                        }
                    }
                    
                }
            }
            // TODO: Est-ce que le restaurant a UNIQUEMENT des plats végés?
            bool? toutVege = null;
            foreach (Restaurant resto in listeResto)
            {
                if (resto.Nom.ToString() == nomDuResto)
                {
                    toutVege = true;
                    foreach (Plat plat in resto.Plats)
                    {
                        if (plat.Vegetarien == false)
                        {
                            toutVege = false;
                        }
                    }
                }
            }
            // TODO: Même chose, mais avec Linq
            // Utilisez Where, All et Any
            bool? optionVegeLinq = null;
            var restaurant = listeResto.Where(resto => resto.Nom == nomDuResto);
            optionVegeLinq = restaurant.Any(resto => resto.Plats.Any(plat => plat.Vegetarien));
            bool? toutVegeLinq = null;
            toutVegeLinq = restaurant.All(resto => resto.Plats.All(plat => plat.Vegetarien));
            return new VegetarienViewModel("Status végétarien du restaurant : " + nomDuResto, toutVege, toutVegeLinq, optionVege, optionVegeLinq);
        }

        // Méthode pratique pour utiliser List<>.Sort()
        private int ComparerPrix(Plat platA, Plat platB)
        {
            decimal diff = platA.Prix - platB.Prix;
            if (diff > 0)
                return 1;
            if(diff < 0)
                return -1;
            return 0;
        }

        private PlatsViewModel PlatsVegeOrdeCroissantDePrix()
        {
            // Remplir une liste avec les plats végés en ordre croissant de prix
            // Note: Il y a une méthode ComparerPrix qui est déjà fournie au dessus
            // Remplir la liste avec une boucle
            List<Plat> plats = new List<Plat>();
            foreach (Plat platVégé in _context.Plat)
            {
                if (platVégé.Vegetarien == true)
                {
                    if (ComparerPrix(platVégé, plats[0]) == 0)
                    {
                        plats.Add(platVégé);
                    }
                    else if (ComparerPrix(platVégé, plats[0]) == 1)
                    {
                        plats.Add(platVégé);
                    }
                }
            }
            // Obtenir la liste avec Linq
            // Utilisez Where, OrderBy et ToList
            List<Plat> platsLinq = new List<Plat>();

            return new PlatsViewModel("Quels sont les plats végétariens?", plats, platsLinq);
        }

        private PlatsViewModel PlatsLesPlusChersOrdeDecroissantDePrix(int nbPlats)
        {
            // Remplir une liste avec les plats les plus chers en ordre décroissant
            // La liste doit avoir uniquement [nbPlats] entrées
            // Utilisez OrderByDescending, Take et ToList
            List<Plat> platsLesPlusChers = new List<Plat>();
            List<Plat> platsLinq = new List<Plat>();
            
            return new PlatsViewModel("Quels sont les plats les plus chers?", platsLesPlusChers, platsLinq);
        }

    }
}
