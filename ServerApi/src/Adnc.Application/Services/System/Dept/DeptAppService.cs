﻿using AutoMapper;
using EasyCaching.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Adnc.Application.Dtos;
using Adnc.Common;
using Adnc.Common.Models;
using Adnc.Core.DomainServices;
using Adnc.Core.Entities;
using Adnc.Core.IRepositories;
using Adnc.Common.Helper;

namespace Adnc.Application.Services
{
    public class DeptAppService : AppService, IDeptAppService
    {
        private readonly IMapper _mapper;
        private readonly IHybridCachingProvider _cache;
        private readonly IEasyCachingProvider _locaCahce;
        private readonly IEasyCachingProvider _redisCache;
        private readonly IEfRepository<SysDept> _deptRepository;
        private readonly ISystemManagerService _systemManagerService;

        public DeptAppService(IMapper mapper
            , IHybridProviderFactory hybridProviderFactory
            , IEasyCachingProviderFactory simpleProviderFactory
            , IEfRepository<SysDept> deptRepository
            , ISystemManagerService systemManagerService)
        {
            _mapper = mapper;
            _cache = hybridProviderFactory.GetHybridCachingProvider(EasyCachingConsts.HybridCaching);
            _locaCahce = simpleProviderFactory.GetCachingProvider(EasyCachingConsts.LocalCaching);
            _redisCache = simpleProviderFactory.GetCachingProvider(EasyCachingConsts.RemoteCaching);
            _deptRepository = deptRepository;
            _systemManagerService = systemManagerService;
        }

        public async Task<int> Delete(long Id)
        {
            var depts1 = (await _locaCahce.GetAsync<List<DeptNodeDto>>(EasyCachingConsts.DetpListCacheKey)).Value;
            var depts2 = (await _redisCache.GetAsync<List<DeptNodeDto>>(EasyCachingConsts.DetpListCacheKey)).Value;
            var depts3 = (await _cache.GetAsync<List<DeptNodeDto>>(EasyCachingConsts.DetpListCacheKey)).Value;

            int result = await _systemManagerService.DeleteDept(Id);
            return await Task.FromResult(result);
        }

        public async Task<List<DeptNodeDto>> GetList()
        {
            List<DeptNodeDto> result = new List<DeptNodeDto>();

            var depts = await _deptRepository.SelectAsync(d => d, x => true);
            if (depts.Any())
            {
                var deptNodes = _mapper.Map<List<DeptNodeDto>>(depts);
                var dictDepts = deptNodes.ToDictionary(x => x.ID);
                foreach (var pair in dictDepts)
                {
                    var currentDept = pair.Value;
                    var parentDept = deptNodes.FirstOrDefault(x => x.ID == currentDept.Pid);
                    if (parentDept != null)
                    {
                        parentDept.Children.Add(currentDept);
                    }
                    else
                    {
                        result.Add(currentDept);
                    }
                }
            }

            return result;
        }

        public async Task<int> Save(DeptSaveInputDto saveDto)
        {
            int result = 0;

            if (string.IsNullOrWhiteSpace(saveDto.FullName))
            {
                throw new BusinessException(new ErrorModel(ErrorCode.BadRequest, "请输入部门全称"));
            }

            if (string.IsNullOrWhiteSpace(saveDto.SimpleName))
            {
                throw new BusinessException(new ErrorModel(ErrorCode.BadRequest,"请输入部门简称"));
            }

            if (saveDto.ID == 0)
            {
                var dept = _mapper.Map<SysDept>(saveDto);
                dept.ID = IdGeneraterHelper.GetNextId(IdGeneraterKey.DEPT);
                await this.SetDeptPids(dept);
                result = await _deptRepository.InsertAsync(dept);
            }
            else
            {
                var oldDept = await _deptRepository.FetchAsync(d => d, x => x.ID == saveDto.ID);
                oldDept.Pid = saveDto.Pid;
                oldDept.SimpleName = saveDto.SimpleName;
                oldDept.FullName = saveDto.FullName;
                oldDept.Num = saveDto.Num;
                oldDept.Tips = saveDto.Tips;
                await this.SetDeptPids(oldDept);

                result = await _deptRepository.UpdateAsync(oldDept);
            }

            return  await Task.FromResult(result);
        }

        private async Task SetDeptPids(SysDept sysDept)
        {
            if (sysDept.Pid.HasValue && sysDept.Pid.Value > 0)
            {
                var dept = await _deptRepository.FetchAsync(d => new { d.ID, d.Pid, d.Pids }, x => x.ID == sysDept.Pid.Value);
                string pids = dept?.Pids ?? "";
                sysDept.Pids = $"{pids}[{sysDept.Pid}],";
            }
            else
            {
                sysDept.Pid = 0;
                sysDept.Pids = "[0],";
            }
        }
    }
}
