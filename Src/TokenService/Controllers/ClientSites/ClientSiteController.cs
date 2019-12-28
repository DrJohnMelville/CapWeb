using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TokenService.Configuration.IdentityServer;
using TokenService.Data;
using TokenService.Data.ClientData;

namespace TokenService.Controllers.ClientSites
{
    public class ClientSiteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEnumerable<IInvalidateClients> invalidateClients;

        public ClientSiteController(ApplicationDbContext context, IEnumerable<IInvalidateClients> invalidateClients)
        {
            _context = context;
            this.invalidateClients = invalidateClients;
        }

        // GET: ClientSite
        public async Task<IActionResult> Index()
        {
            return View(await _context.ClientSites.ToListAsync());
        }

        // GET: ClientSite/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientSite = await _context.ClientSites
                .FirstOrDefaultAsync(m => m.ShortName == id);
            if (clientSite == null)
            {
                return NotFound();
            }

            return View(clientSite);
        }

        // GET: ClientSite/Create
        public IActionResult Create()
        {
            return View(new ClientSite() {ClientSecret = CryptoRandom.CreateUniqueId()});
        }

        // POST: ClientSite/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FriendlyName,ShortName,ClientSecret,BaseUri,RedirectExtenstions,FrontChannelLogoutExtension,PostLogoutRedirectExtensions,AllowedScopes")] ClientSite clientSite)
        {
            if (ModelState.IsValid)
            {
                _context.Add(clientSite);
                await _context.SaveChangesAsync();
                invalidateClients.Invalidate();
                return RedirectToAction(nameof(Index));
            }
            return View(clientSite);
        }

        // GET: ClientSite/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientSite = await _context.ClientSites.FindAsync(id);
            if (clientSite == null)
            {
                return NotFound();
            }
            return View(clientSite);
        }

        // POST: ClientSite/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("FriendlyName,ShortName,ClientSecret,BaseUri,RedirectExtenstions,FrontChannelLogoutExtension,PostLogoutRedirectExtensions,AllowedScopes")] ClientSite clientSite)
        {
            if (id != clientSite.ShortName)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clientSite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientSiteExists(clientSite.ShortName))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                invalidateClients.Invalidate();
                return RedirectToAction(nameof(Index));
            }
            return View(clientSite);
        }

        // GET: ClientSite/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientSite = await _context.ClientSites
                .FirstOrDefaultAsync(m => m.ShortName == id);
            if (clientSite == null)
            {
                return NotFound();
            }

            return View(clientSite);
        }

        // POST: ClientSite/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var clientSite = await _context.ClientSites.FindAsync(id);
            _context.ClientSites.Remove(clientSite);
            await _context.SaveChangesAsync();
            invalidateClients.Invalidate();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientSiteExists(string id)
        {
            return _context.ClientSites.Any(e => e.ShortName == id);
        }
    }
}
